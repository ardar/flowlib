using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleDemo.ConsoleClient.Controls.Interfaces
{
    public interface IFocusable
    {
        /// <summary>
        /// Sets control in focus.
        /// </summary>
        /// <returns>
        /// 0 = User has finished editing/viewing control
        /// 1 = User wants to make next control focus
        /// </returns>
        int Focus();
    }
}
