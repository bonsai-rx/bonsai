using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    [DefaultProperty("Path")]
    [Description("Returns the names of the subdirectories that match the specified search pattern.")]
    public class GetDirectories : Source<string[]>
    {
        public GetDirectories()
        {
            Path = ".";
            SearchPattern = "*";
        }

        [Description("The path to search.")]
        [Editor("Bonsai.Design.FolderNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Path { get; set; }

        [Description("The search string used to match against the names of subdirectories in the path.")]
        public string SearchPattern { get; set; }

        [Description("Specifies whether the search should include all subdirectories.")]
        public SearchOption SearchOption { get; set; }

        public override IObservable<string[]> Generate()
        {
            return Observable.Return(Directory.GetDirectories(Path, SearchPattern, SearchOption));
        }
    }
}
