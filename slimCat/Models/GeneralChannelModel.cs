﻿#region Copyright

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneralChannelModel.cs">
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

namespace Slimcat.Models
{
    #region Usings

    using System;
    using System.Collections.Specialized;
    using Utilities;

    #endregion

    /// <summary>
    ///     The general channel model.
    /// </summary>
    public sealed class GeneralChannelModel : ChannelModel
    {
        #region Fields

        private string description;

        private int lastAdCount;

        private DateTime lastUpdate;

        private int userCount;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GeneralChannelModel" /> class.
        /// </summary>
        /// <param name="channelName">
        ///     The channel_name.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="users">
        ///     The users.
        /// </param>
        /// <param name="mode">
        ///     The mode.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public GeneralChannelModel(
            string channelName, ChannelType type, int users = 0, ChannelMode mode = ChannelMode.Both)
            : base(channelName, type, mode)
        {
            try
            {
                if (users < 0)
                    throw new ArgumentOutOfRangeException("users", "Users cannot be a negative number");

                UserCount = users;

                CharacterManager = new ChannelCharacterManager();
                Settings = new ChannelSettingsModel();

                // the message count now faces the user, so when we reset it it now requires a UI update
                Messages.CollectionChanged += (s, e) =>
                    {
                        if (e.Action != NotifyCollectionChangedAction.Reset)
                            return;

                        LastReadCount = Messages.Count;
                        UpdateBindings();
                    };

                Ads.CollectionChanged += (s, e) =>
                    {
                        if (e.Action != NotifyCollectionChangedAction.Reset)
                            return;

                        LastReadAdCount = Ads.Count;
                        UpdateBindings();
                    };
            }
            catch (Exception ex)
            {
                ex.Source = "General Channel Model, init";
                Exceptions.HandleException(ex);
            }
        }

        #endregion

        #region Public Properties

        public ICharacterManager CharacterManager { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether can close.
        /// </summary>
        public override bool CanClose
        {
            get { return (Id != "Home") && IsSelected; }
        }

        /// <summary>
        ///     Gets the composite unread count.
        /// </summary>
        public int CompositeUnreadCount
        {
            get { return Math.Max(Unread + UnreadAds, 0); }
        }

        /// <summary>
        ///     Gets or sets the motd.
        /// </summary>
        public string Description
        {
            get { return description; }

            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        ///     Gets the display number.
        /// </summary>
        public int DisplayNumber
        {
            get { return UserCount; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether is selected.
        /// </summary>
        public override bool IsSelected
        {
            get { return base.IsSelected; }

            set
            {
                base.IsSelected = value;
                if (value)
                    LastReadAdCount = Ads.Count;
            }
        }

        /// <summary>
        ///     Gets or sets the last read ad count.
        /// </summary>
        public int LastReadAdCount
        {
            get { return lastAdCount; }

            set
            {
                if (lastAdCount == value)
                    return;

                lastAdCount = value;
                UpdateBindings();
            }
        }

        /// <summary>
        ///     Gets a value indicating whether needs attention.
        /// </summary>
        public override bool NeedsAttention
        {
            get
            {
                if (!IsSelected && NeedsAttentionOverride)
                    return true; // flash for ding words

                if (Settings.MessageNotifyLevel == 0)
                    return false; // terminate early upon user request

                if (Settings.MessageNotifyOnlyForInteresting)
                    return base.NeedsAttention;

                return base.NeedsAttention || (UnreadAds >= Settings.FlashInterval);
            }
        }

        /// <summary>
        ///     Gets the unread Ads.
        /// </summary>
        public int UnreadAds
        {
            get { return Ads.Count - lastAdCount; }
        }

        /// <summary>
        ///     Gets or sets the user count.
        /// </summary>
        public int UserCount
        {
            get { return CharacterManager.CharacterCount == 0 ? userCount : CharacterManager.CharacterCount; }

            set
            {
                userCount = value;
                UpdateBindings();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The add message.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="isOfInterest">
        ///     The is of interest.
        /// </param>
        public override void AddMessage(IMessage message, bool isOfInterest = false)
        {
            var messageCollection = message.Type == MessageType.Ad ? Ads : Messages;

            while (messageCollection.Count >= ApplicationSettings.BackLogMax)
            {
                messageCollection[0].Dispose();
                messageCollection.RemoveAt(0);
            }

            messageCollection.Add(message);

            if (IsSelected)
            {
                if (message.Type == MessageType.Normal)
                    LastReadCount = messageCollection.Count;
                else
                    LastReadAdCount = messageCollection.Count;
            }
            else if (messageCollection.Count >= ApplicationSettings.BackLogMax)
            {
                if (message.Type == MessageType.Normal)
                    LastReadCount--;
                else
                    LastReadAdCount--;
            }
            else if (!IsSelected)
                UnreadContainsInteresting = isOfInterest;

            UpdateBindings();
        }

        #endregion

        #region Methods

        protected override void Dispose(bool isManaged)
        {
            Settings = new ChannelSettingsModel();
            base.Dispose(isManaged);
        }

        /// <summary>
        ///     The update bindings.
        /// </summary>
        protected override void UpdateBindings()
        {
            base.UpdateBindings();
            OnPropertyChanged("CompositeUnreadCount");
        }

        #endregion
    }
}