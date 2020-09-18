using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Sets the active texture index of the specified texture array.")]
    public class UpdateTextureArray : Sink
    {
        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the texture array to update.")]
        public string TextureName { get; set; }

        [Description("The index of the active texture to set.")]
        public int Index { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Create<TSource>(observer =>
            {
                var name = TextureName;
                var texture = default(TextureArray);
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A texture name must be specified.");
                }

                return source.CombineEither(
                    ShaderManager.WindowSource.Do(window =>
                    {
                        window.Update(() =>
                        {
                            try { texture = (TextureArray)window.ResourceManager.Load<Texture>(name); }
                            catch (Exception ex) { observer.OnError(ex); }
                        });
                    }),
                    (input, window) =>
                    {
                        var index = Index;
                        window.Update(() => texture.SetActiveTexture(index));
                        return input;
                    }).SubscribeSafe(observer);
            });
        }
    }
}
