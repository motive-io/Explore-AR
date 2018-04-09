// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class LocationValuablesCollectionProcessor : ScriptResourceProcessor<LocationValuablesCollection>
    {
        int m_opsInProgress = 0;

        public override void ActivateResource(ResourceActivationContext context, LocationValuablesCollection resource)
        {
            LocationValuablesCollectionManager.Instance.AddLocationValuablesCollection(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, LocationValuablesCollection resource)
        {
            // Reset the state for this instance if deactiating
            LocationValuablesCollectionManager.Instance.RemoveLocationValuablesCollection(context, resource);
        }

        public override void UpdateResource(ResourceActivationContext context, LocationValuablesCollection resource)
        {
            // Do not reset the state if we're only updating
            LocationValuablesCollectionManager.Instance.RemoveLocationValuablesCollection(context, resource);
            LocationValuablesCollectionManager.Instance.AddLocationValuablesCollection(context, resource);
        }

        public override void BeginOperation()
        {
            bool callBeginOp = false;

            lock (this)
            {
                callBeginOp = m_opsInProgress == 0;

                m_opsInProgress++;
            }

            if (callBeginOp)
            {
                LocationValuablesCollectionManager.Instance.BeginUpdate();
            }

            base.BeginOperation();
        }

        public override void EndOperation()
        {
            bool callEndOp = false;

            lock (this)
            {
                m_opsInProgress--;

                callEndOp = m_opsInProgress == 0;
            }

            if (callEndOp)
            {
                LocationValuablesCollectionManager.Instance.EndUpdate();
            }

            base.EndOperation();
        }
    }
}