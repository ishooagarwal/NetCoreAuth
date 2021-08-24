using AspNetNull.Persistance.Exceptions;
using AspNetNull.Persistance.Models;
using AspNetNull.Persistance.Models.Email;
using AspNetNull.Persistance.Models.ErrorResponses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace AspNetNull.Persistance.Helpers
{
    public static class ProjectHelpers
    {
        /// <summary>
        /// Handles Success and Failure Response. Populates the given Content and HttpStatusCode.
        /// </summary>
        /// <param name="response">Response Model.</param>
        /// <returns>Return the HttpResponse Object result.</returns>
        public static ObjectResult ObjectResultFromStatus(this Response response)
        {
            ObjectResult result = new ObjectResult(response)
            {
                StatusCode = (int)response.HttpStatusCode
            };

            return result;
        }

        /// <summary>
        /// Generate Task<IActionResult> result.
        /// </summary>
        /// <param name="rawresult">Class inhenriting from Response model.</param>
        /// <param name="statusCode">Optional status code.</param>
        /// <returns>a ObjectResult object.</returns>
        public static ObjectResult GenerateResult(dynamic rawresult, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            ObjectResult result = new ObjectResult(rawresult)
            {
                StatusCode = (int)statusCode
            };

            return result;
        }

        /// <summary>
        /// Convert an ApiException to a ServiceException
        /// If the ApiException has a content, give back a ServiceExceptionWithHttpResponse.
        /// </summary>
        /// <param name="ex">Source ValidationException.</param>
        /// <returns>a Errors object.</returns>
        public static Errors GetValidationError(this ValidationException ex)
        {
            return GenericError(ex, HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Get UnAuthorized 401 error.
        /// </summary>
        /// <param name="ex">Source UnauthorizedAccessException.</param>
        /// <returns>a Errors Object.</returns>
        public static Errors GetUnAuthorizedError(this UnauthorizedAccessException ex)
        {
            return GenericError(ex, HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Get InternalServerError 500 error.
        /// </summary>
        /// <param name="ex">Source Exception.</param>
        /// <returns>a Errors Object.</returns>
        public static Errors GetInternalServerError(this Exception ex)
        {
            return GenericError(ex, HttpStatusCode.InternalServerError);
        }

        public static Errors GenericError(Exception ex, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            Errors errors = new Errors() { HttpStatusCode = statusCode };
            Error errorResponse = new Error();
            errorResponse.errorMessages = ex.Message;
            errors.Error = new Error[] { errorResponse };
            return errors;
        }

        /// <summary>
        /// Generate unique guid Id for string datatypes.
        /// </summary>
        /// <param name="value">String value.</param>
        public static string GenerateUniqueId(this string value)
        {
            value = Guid.NewGuid().ToString();
            return value;
        }

        public static string MaskEmail(this string value)
        {
            var email = value.Split('@');
            int length = email[0].Length;

            string maskedEmail = email[0].Substring(0,4);
            maskedEmail = maskedEmail + "XXXX";
            string remainingEmail = email[0].Substring(maskedEmail.Length, length - maskedEmail.Length);
            maskedEmail = string.Concat(maskedEmail, remainingEmail);

            return maskedEmail + email[1];
        }

        public static void SendMail(Message message, string secretKey)
        {
            #region formatter
            string text = string.Format("Please click on this link to {0}: {1}", message.Subject, message.Body);
            string html = "Please confirm your account by clicking this link: <a href=\"" + message.Body + "\">link</a><br/>";

            html += HttpUtility.HtmlEncode(@"Or click on the copy the following link on the browser:" + message.Body);
            #endregion

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(message.SenderEmail);
            msg.To.Add(new MailAddress(message.Destination));
            msg.Subject = message.Subject;
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", Convert.ToInt32(587));
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(message.SenderEmail, secretKey);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = credentials;
            smtpClient.EnableSsl = true;
            smtpClient.Send(msg);
        }

        public static string MergeUserIdWithCode(string code, string userId)
        {
            int midLength = code.Length / 2;

            string splitCodePart1 = code.Substring(0, midLength);
            string splitCodePart2 = code.Substring(midLength, code.Length - midLength);

            string finalCode = splitCodePart1 + userId + splitCodePart2;

            return finalCode;
        }

        public static string SplitUserIdWithCode(string code, out string userId, int expectedIdLength)
        {
            int midLength = code.Length / 2;

            string splitCodePart1 = code.Substring(0, midLength - expectedIdLength/2);
            userId = code.Substring(splitCodePart1.Length, expectedIdLength);
            string splitCodePart3 = code.Substring(splitCodePart1.Length + userId.Length, code.Length - (splitCodePart1.Length + userId.Length));

            string token = (splitCodePart1 + splitCodePart3).Replace(" ", "+");

            return token;
        }

        

    }
}
