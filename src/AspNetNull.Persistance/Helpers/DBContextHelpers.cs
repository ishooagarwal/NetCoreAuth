using AspNetNull.Persistance.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Helpers
{
    public static class DBContextHelpers
    {
        public static async Task Detach<TModel>(this ApplicationDBContext dbContext, TModel model) where TModel : class
        {
            dbContext.Entry(model).State = EntityState.Detached;
        }

        public static async Task AssignConcurrencyOriginalValue<TModel, TValue>(this ApplicationDBContext dbContext, TModel model, TValue value) where TModel:class
        {
            dbContext.Entry(model).Property("ConcurrencyStamp").OriginalValue = value;
        }

        /// <summary>
        /// Handles all update tasks like Detaching the model from context and mapping concurrency value.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dbContext">Instance of type ApplicationDBContext.</param>
        /// <param name="model">Instance of model which needs to be detaced.</param>
        /// <param name="value">concurrency value</param>
        /// <returns></returns>
        public static async Task HandleUpdateTasks<TModel, TValue>(this ApplicationDBContext dbContext, TModel model, TValue value) where TModel : class
        {
            var detachTask = dbContext.Detach(model);
            var concurrencyValueTask = dbContext.AssignConcurrencyOriginalValue(model, value);

            await Task.WhenAll(detachTask, concurrencyValueTask);
        }
    }
}
