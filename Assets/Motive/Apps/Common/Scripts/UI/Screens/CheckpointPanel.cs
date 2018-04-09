// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Apps;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays a list of save points.
    /// </summary>
    /// <seealso cref="Motive.UI.Framework.TablePanel" />
    public class CheckpointPanel : TablePanel
    {
        public CheckpointTableItem ItemPrefab;
        public OptionsDialogPanel ConfirmDialog;

        public override void Populate()
        {
            Table.Clear();

            var checkpoints = SavePointManager.Instance.Checkpoints;

            foreach (var checkpoint in checkpoints)
            {
                var item = Table.AddItem(ItemPrefab);

                item.Title.text = checkpoint.Checkpoint.Title;
                item.Time.text = checkpoint.Time.ToString();

                item.OnSelected.AddListener(() => { Select(checkpoint); });
            }

            base.Populate();
        }

        private void Select(SavePointManager.SavePointState checkpoint)
        {
            if (ConfirmDialog)
            {
                ConfirmDialog.Show(new string[] { "OK" }, (opt) =>
                    {
                        if (opt == "OK")
                        {
                            SavePointManager.Instance.Restore(checkpoint.Checkpoint);
                            PanelManager.Instance.HideAll();
                        }
                    });
            }
            else
            {
                SavePointManager.Instance.Restore(checkpoint.Checkpoint);
                PanelManager.Instance.HideAll();
            }
        }
    }
}