using System;

namespace PagingEx
{
    internal class ActivityModel
    {
        public ActivityModel(Type activityType, object parameter)
        {
            Type = activityType;
            Parameter = parameter;
            Key = Guid.NewGuid().ToString();
        }

        public object Parameter { get; internal set; }

        public Activity Activity { get; private set; }

        public Type Type { get; }

        private string Key { get; }

        internal Activity GetActivity(ActivityContainer activityContainer)
        {
            if (Activity != null)
            {
                return Activity;
            }

            if (!(Activator.CreateInstance(Type) is Activity activity))
            {
                throw new InvalidOperationException(
                    $"The base type is not an {nameof(Activity)}. Change the base type from Activity to {nameof(Activity)}. ");
            }

            activity.SetContainer(activityContainer);
            activity.OnCreate(Parameter);
            Activity = activity;

            return Activity;
        }


        internal void Release()
        {
            Activity.OnDestroy();
            Activity = null;
        }
    }
}