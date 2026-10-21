using System;
using System.Threading.Tasks;

namespace XForms
{
    /// <summary>
    /// A page shown modally that closes with a result: show it with
    /// <see cref="Application.ShowModalAsync{TResult}(ModalPage{TResult})"/>,
    /// which completes with the result.
    /// </summary>
    public abstract class ModalPage<TResult> : Page
    {
        /// <summary>
        /// Closes the page with a result.
        /// </summary>
        protected Task CloseAsync(
            TResult result)
        {
            return this.Application.CloseModalAsync(this, result);
        }
    }
}
