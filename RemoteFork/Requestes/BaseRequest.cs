namespace RemoteFork.Requestes {
    internal abstract class BaseRequest {
        protected string text;

        protected BaseRequest(string text) {
            this.text = text;
        }

        public abstract string Execute();
    }
}
