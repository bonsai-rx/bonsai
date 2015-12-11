using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Video
{
    [Description("Publishes the sequence of images as a JPEG image server.")]
    public class JpegServer : Sink<IplImage>
    {
        [Description("The URL which will provide JPEG image files.")]
        public string SourceUrl { get; set; }

        static IObservable<HttpListenerContext> GetContext(HttpListener listener)
        {
            return Observable
                .Defer(() => listener.GetContextAsync().ToObservable())
                .Repeat()
                .Retry();
        }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Publish(ps =>
            {
                return ps.Merge(Observable.Using(
                    () =>
                    {
                        var listener = new HttpListener();
                        listener.Prefixes.Add(SourceUrl);
                        listener.Start();
                        return listener;
                    },
                    listener =>
                    {
                        var clients = GetContext(listener).Publish().RefCount();
                        var jpgs = ps.Select(input =>
                        {
                            var data = CV.EncodeImage(".jpg", input);
                            var result = new byte[data.Cols];
                            using (var header = Mat.CreateMatHeader(result))
                            {
                                CV.Copy(data, header);
                            }
                            return result;
                        }).PublishReconnectable().RefCount();

                        return clients.SelectMany(context =>
                        {
                            var response = context.Response;
                            var stream = response.OutputStream;
                            return jpgs.FirstAsync().Do(data =>
                            {
                                stream.Write(data, 0, data.Length);
                                response.Close();
                            });
                        }).Retry().IgnoreElements().Select(x => default(IplImage));
                    }));
            });
        }
    }
}
