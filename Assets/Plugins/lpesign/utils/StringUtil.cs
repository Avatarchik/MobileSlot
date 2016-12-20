using System.Text;

namespace lpesign
{
    public class StringUtil
    {
    }
	
	static public class StringExtension
	{
		public static void Clear( this StringBuilder sb )
		{
			sb.Length = 0;
			sb.Capacity = 0;
		}
	}
}
