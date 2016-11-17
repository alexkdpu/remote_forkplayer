namespace RemoteFork.Plugins {
    public class BaseItem {
        public string Name;
        public string Link;
        public string ImageLink;
        public string Description;
        public ItemType Type = ItemType.DIRECTORY;

        public BaseItem() {
        }

        public BaseItem(BaseItem item) {

            Name = item.Name;
            Link = item.Link;
            ImageLink = item.ImageLink;
            Description = item.Description;
            Type = item.Type;
        }
    }

    public enum ItemType {
        DIRECTORY = 0,
        FILE = 1,
        SEARCH = 2
    }
}
