using System;
using System.Reflection;

namespace MudRunnerModManager.Common
{
	public class AppVersion
	{

		public AppVersion(int major, int minor, int build)
		{
			Major = major;
			Minor = minor;
			Build = build;
		}

		public AppVersion(Version version) : this(version.Major, version.Minor, version.Build)
		{
		}



		public int Major { get; }

		public int Minor { get; }

		public int Build { get; }


		public static AppVersion GetVersion()
		{
			var ver = Assembly.GetExecutingAssembly().GetName().Version;
			if (ver == null)
				throw new Exception("Version is not set!");

			return new AppVersion(ver);
		}


		public static bool operator==(AppVersion? appVersionA, object? appVersionB)
		{
			if (appVersionA is null)
				return appVersionB is null;

			return appVersionA.Equals(appVersionB);
		}

		public static bool operator !=(AppVersion? appVersionA, object? appVersionB)
		{
			if (appVersionA is null)
				return appVersionB is not null;

			return !appVersionA.Equals(appVersionB);
		}

		public static bool operator >(AppVersion appVersionA, AppVersion appVersionB)
		{
			if (appVersionA.Major > appVersionB.Major
				|| (appVersionA.Major == appVersionB.Major && appVersionA.Minor > appVersionB.Minor)
				|| (appVersionA.Major == appVersionB.Major && appVersionA.Minor == appVersionB.Minor && appVersionA.Build > appVersionB.Build))
				return true;

			return false;
		}

		public static bool operator <(AppVersion appVersionA, AppVersion appVersionB)
		{
			if (appVersionA.Major < appVersionB.Major
				|| (appVersionA.Major == appVersionB.Major && appVersionA.Minor < appVersionB.Minor)
				|| (appVersionA.Major == appVersionB.Major && appVersionA.Minor == appVersionB.Minor && appVersionA.Build < appVersionB.Build))
				return true;

			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is not null && obj is AppVersion ver)
			{
				return GetHashCode() == ver.GetHashCode();
			}

			return false;			
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override string ToString()
		{
			return $"{Major}.{Minor}.{Build}";
		}
	}
}
