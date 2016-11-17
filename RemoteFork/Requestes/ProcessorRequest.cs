using RemoteFork.Server;

namespace RemoteFork.Requestes {
    internal abstract class ProcessorRequest : BaseRequest {
        protected HttpProcessor processor;

        protected ProcessorRequest(string text, HttpProcessor processor) : base(text) {
            this.processor = processor;
        }
    }
}
