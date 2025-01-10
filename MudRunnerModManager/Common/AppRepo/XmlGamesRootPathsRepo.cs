using MudRunnerModManager.Common.XmlWorker;
using System;
using System.Linq;

namespace MudRunnerModManager.Common.AppRepo
{
	public class XmlGamesRootPathsRepo : IGamesRootPathsRepo
	{
		private readonly string _filePath;

		public XmlGamesRootPathsRepo(string filePath)
		{
			_filePath = filePath;
		}

		public GameRootPath? Get(string gameName)
		{
			if(string.IsNullOrWhiteSpace(gameName)) 
				return null;

			XmlDoc xmlGRP = new(_filePath);

			if (!xmlGRP.Exists)
				return null;

			xmlGRP.Load();

			if (xmlGRP.IsEmpty)
				return null;

			if (TryGetByGameName(xmlGRP, gameName, out XmlElem xmlElem))
			{
				return new GameRootPath
				(
					xmlElem.Attributes.First(atr => atr.Name == AppConsts.PATH).Value, 
					xmlElem.Attributes.First(atr => atr.Name == AppConsts.NAME).Value
				);
			}

			return null;			
		}

		public void Save(GameRootPath gameRootPath)
		{
			if (gameRootPath == null
				|| string.IsNullOrWhiteSpace(gameRootPath.Path)
				|| string.IsNullOrWhiteSpace(gameRootPath.GameName))
				throw new ArgumentException($"{nameof(GameRootPath.Path)} or {nameof(GameRootPath.GameName)} can't be null or empty");

			XmlDoc xmlGRP = new(_filePath);

			xmlGRP.LoadOrCreate();

			void AddRoot()
			{
				XmlElem grpElem = new(AppConsts.GAMES_ROOT_PATHS);
				xmlGRP.AddRootXmlElem(grpElem);
			}

			if (xmlGRP.IsEmpty)
			{
				AddRoot();
			}
			else if (!xmlGRP.IsPresentElem(new(AppConsts.CHAPTERS)))
			{
				xmlGRP.Clear();
				AddRoot();
			}

			if(TryGetByGameName(xmlGRP, gameRootPath.GameName, out XmlElem presentGRPElem))
				xmlGRP.ReplaceXmlElem(presentGRPElem, CreateGRPElem(gameRootPath), AppConsts.GAMES_ROOT_PATHS);
			else
				xmlGRP.AddXmlElem(CreateGRPElem(gameRootPath), AppConsts.GAMES_ROOT_PATHS);

			xmlGRP.Save();
		}

		public bool IsPresent(GameRootPath gameRootPath)
		{
			XmlDoc xmlGRP = new(_filePath);

			if(!xmlGRP.Exists)
				return false;

			xmlGRP.Load();

			if(xmlGRP.IsEmpty)
				return false;

			return xmlGRP.IsPresentElem(CreateGRPElem(gameRootPath));
		}

		public bool IsPresent(string gameName)
		{
			XmlDoc xmlGRP = new(_filePath);

			if (!xmlGRP.Exists)
				return false;

			xmlGRP.Load();

			if (xmlGRP.IsEmpty)
				return false;

			return TryGetByGameName(xmlGRP, gameName, out _);
		}

		private XmlElem CreateGRPElem(GameRootPath gameRootPath)
		{
			XmlElem grpElem = new(AppConsts.GAME);
			grpElem.Attributes.Add(new XmlElemAttribute(AppConsts.PATH, gameRootPath.Path));
			grpElem.Attributes.Add(new XmlElemAttribute(AppConsts.NAME, gameRootPath.GameName));
			return grpElem;
		}

		private bool TryGetByGameName(XmlDoc xmlGRP, string gameName, out XmlElem xmlElem)
		{
			bool SearchByGameName(XmlElem elem)
			{
				if (elem.Name == AppConsts.GAME)
				{
					var gameNameAttr = elem.Attributes.FirstOrDefault(atr => atr.Name == AppConsts.NAME);
					if (gameNameAttr != null && gameNameAttr.Value == gameName)
						return true;
				}

				return false;
			}

			xmlElem = xmlGRP.GetXmlItem<XmlElem>(SearchByGameName) ?? new XmlElem(string.Empty);

			return xmlElem.Name != string.Empty;
		}
	}
}
