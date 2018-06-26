// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Used for background -> foreground transitions when you don't want
    /// the calls from the background to queue up (as they do with ThreadHelper.CallOnMainThread).
    /// </summary>
	public class ExclusiveUpdater
	{
		Action m_currentAction;

		public ExclusiveUpdater ()
		{
		}

		public void Cancel()
		{
			m_currentAction = null;
		}

        public void Update(Action updateAction)
		{
			bool callHelper = (m_currentAction == null);

			m_currentAction = updateAction;

			if (callHelper) {
				ThreadHelper.Instance.CallOnMainThread(() => {
					if (m_currentAction != null) {
						var action = m_currentAction;
						m_currentAction = null;
						action();
					}
				});
			}
		}
	}
}

