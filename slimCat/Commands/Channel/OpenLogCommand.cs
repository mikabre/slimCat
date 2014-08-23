﻿#region Copyright

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BroadcastCommand.cs">
//    Copyright (c) 2013, Justin Kadrovach, All rights reserved.
//   
//    This source is subject to the Simplified BSD License.
//    Please see the License.txt file for more information.
//    All other rights reserved.
//    
//    THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
//    KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//    IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//    PARTICULAR PURPOSE.
// </copyright>
//  --------------------------------------------------------------------------------------------------------------------

#endregion

namespace slimCat.Services
{
    using System.Collections.Generic;
    using Utilities;

    public partial class UserCommandService
    {
        private void OnOpenLogRequested(IDictionary<string, object> command)
        {
            logger.OpenLog(false, model.CurrentChannel.Title, model.CurrentChannel.Id);
        }

        private void OnOpenLogFolderRequested(IDictionary<string, object> command)
        {
            if (command.ContainsKey(Constants.Arguments.Channel))
            {
                var toOpen = command.Get(Constants.Arguments.Channel);
                if (!string.IsNullOrWhiteSpace(toOpen))
                    logger.OpenLog(true, toOpen, toOpen);
            }
            else
                logger.OpenLog(true, model.CurrentChannel.Title, model.CurrentChannel.Id);
        }
    }
}