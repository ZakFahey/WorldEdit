namespace WorldEditSchematicTranslator;

internal partial class Program
{
    private sealed class EntitiesReader : IEnumerable<EntityReaderState>
    {
        private sealed class EntitiesReaderEnumerator : IEnumerator<EntityReaderState>
        {
            private readonly BinaryReader BinaryReader;
            private EntityReaderState Curr = new();
            public EntityReaderState Current => Curr;
            object System.Collections.IEnumerator.Current => Curr;
            public EntitiesReaderEnumerator(BinaryReader BinaryReader) =>
                this.BinaryReader = BinaryReader;

            private bool NextType() => (++Curr.Type <= EntityType.FoodPlate);
            public bool MoveNext()
            {
                if (!Curr.Read)
                    return NextType();

                if (Curr.ReadCount >= Curr.TotalCount)
                {
                    if (!NextType())
                        return false;

                    Curr.ReadCount = 0;
                    try { Curr.TotalCount = BinaryReader.ReadInt32(); }
                    catch (IOException)
                    {
                        Curr.TotalCount = 0;
                        Curr.Read = false;
                        return true;
                    }
                }

                if (Curr.ReadCount < Curr.TotalCount)
                {
                    Curr.Entity = (Curr.Type switch
                    {
                        EntityType.Sign => SignData.Instance.Read(BinaryReader),
                        EntityType.Chest => ChestData.Instance.Read(BinaryReader),
                        EntityType.ItemFrame => DisplayItemData.Instance.Read(BinaryReader),
                        EntityType.LogicSensor => LogicSensorData.Instance.Read(BinaryReader),
                        EntityType.TargetDummy => EmptyData.Instance.Read(BinaryReader),
                        EntityType.WeaponRack => DisplayItemData.Instance.Read(BinaryReader),
                        EntityType.Pylon => EmptyData.Instance.Read(BinaryReader),
                        EntityType.Mannequin => DisplayItemsData.Instance.Read(BinaryReader),
                        EntityType.HatRack => DisplayItemsData.Instance.Read(BinaryReader),
                        EntityType.FoodPlate => DisplayItemData.Instance.Read(BinaryReader),
                        _ => throw new NotImplementedException()
                    });
                    Curr.ReadCount++;
                }
                return true;
            }

            public void Reset() { }
            public void Dispose() { }
        }

        #region Ienumerable realization

        private readonly EntitiesReaderEnumerator Enumerator;
        public EntitiesReader(BinaryReader BinaryReader) =>
            Enumerator = new(BinaryReader);

        public IEnumerator<EntityReaderState> GetEnumerator() => Enumerator;
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Enumerator;

        #endregion
    }
}