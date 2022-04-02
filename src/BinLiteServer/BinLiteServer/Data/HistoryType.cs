using System;

namespace BinLiteServer
{
    [Serializable]
    public enum HistoryType : int
    {
        RealmEnabled = 0,
        RealmDisabled = 1,
        RealmNameChange = 2,
        RealmOwnerChange = 3,
        RealmPermission = 4,

        ItemCreated = 5,
        ItemDeleted = 6,
        ItemUpdate = 7,

        TagCreated = 8,
        TagDeleted = 9,
        TagItemChange = 10,
    }
}
