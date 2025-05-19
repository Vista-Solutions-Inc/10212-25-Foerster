
using System.Windows.Markup;

namespace Foerster.ViewModels.Helpers
{
    public class EnumBindingSourceExtension : MarkupExtension
    {
        public Type EnumType { get; private set; }
        public EnumBindingSourceExtension(Type enumType)
        {
            if (enumType is null || !enumType.IsEnum) throw new Exception("EnuType must not be null and of type Enum");
            EnumType = enumType;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {

            return Enum.GetValues(EnumType);
        }
    }
}
