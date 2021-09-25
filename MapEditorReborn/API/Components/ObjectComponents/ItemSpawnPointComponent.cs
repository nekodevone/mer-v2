﻿namespace MapEditorReborn.API
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.CustomItems.API.Features;
    using InventorySystem.Items.Firearms.Attachments;
    using MEC;

    using Random = UnityEngine.Random;

    /// <summary>
    /// Used for handling ItemSpawnPoint's spawning items.
    /// </summary>
    public class ItemSpawnPointComponent : MapEditorObject
    {
        /// <summary>
        /// Initializes the <see cref="ItemSpawnPointComponent"/>.
        /// </summary>
        /// <param name="itemSpawnPoint">The <see cref="ItemSpawnPointObject"/> to initialize.</param>
        /// <returns>Instance of this compoment.</returns>
        public ItemSpawnPointComponent Init(ItemSpawnPointObject itemSpawnPoint)
        {
            Base = itemSpawnPoint;

            UpdateObject();

            return this;
        }

        public ItemSpawnPointObject Base;

        /// <inheritdoc cref="MapEditorObject.UpdateObject()"/>
        public override void UpdateObject()
        {
            OnDestroy();
            attachedPickups.Clear();

            if (Random.Range(0, 101) > Base.SpawnChance)
                return;

            if (Enum.TryParse(Base.Item, out ItemType parsedItem))
            {
                for (int i = 0; i < Base.NumberOfItems; i++)
                {
                    Item item = new Item(parsedItem);
                    Pickup pickup = item.Spawn(transform.position, transform.rotation);

                    if (pickup.Base is InventorySystem.Items.Firearms.FirearmPickup firearmPickup)
                    {
                        int rawCode = GetAttachmentsCode(Base.AttachmentsCode);
                        uint code = rawCode != -1 ? (item.Base as InventorySystem.Items.Firearms.Firearm).ValidateAttachmentsCode((uint)rawCode) : AttachmentsUtils.GetRandomAttachmentsCode(parsedItem);

                        firearmPickup.NetworkStatus = new InventorySystem.Items.Firearms.FirearmStatus(firearmPickup.NetworkStatus.Ammo, firearmPickup.NetworkStatus.Flags, code);
                    }

                    attachedPickups.Add(pickup);
                }
            }
            else
            {
                for (int i = 0; i < Base.NumberOfItems; i++)
                {
                    Timing.RunCoroutine(SpawnCustomItem());
                }
            }
        }

        private IEnumerator<float> SpawnCustomItem()
        {
            yield return Timing.WaitUntilTrue(() => Round.IsStarted);

            if (CustomItem.TrySpawn(Base.Item, transform.position, out Pickup customItem))
            {
                customItem.Rotation = transform.rotation;
                attachedPickups.Add(customItem);
            }
        }

        private int GetAttachmentsCode(string attachmentsString)
        {
            if (attachmentsString == "-1")
                return -1;

            int attachementsCode = 0;

            if (attachmentsString.Contains("+"))
            {
                string[] array = attachmentsString.Split(new char[] { '+' });

                for (int j = 0; j < array.Length; j++)
                {
                    if (int.TryParse(array[j], out int num))
                    {
                        attachementsCode += num;
                    }
                }
            }
            else
            {
                attachementsCode = int.Parse(attachmentsString);
            }

            return attachementsCode;
        }

        private void OnDestroy()
        {
            foreach (Pickup pickup in attachedPickups)
            {
                pickup.Destroy();
            }
        }

        private List<Pickup> attachedPickups = new List<Pickup>();
    }
}