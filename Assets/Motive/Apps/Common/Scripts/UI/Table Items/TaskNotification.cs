// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Gaming;

namespace Motive.Unity.UI
{
    /// <summary>
    /// A toast notification to alert the user of a new task.
    /// </summary>
    public class TaskNotification : TextImageItem
    {
        public IPlayerTaskDriver TaskDriver;

        public virtual void Populate(IPlayerTaskDriver driver)
        {
            TaskDriver = driver;

            if (driver != null)
            {
                SetText(driver.Task.Title);
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}