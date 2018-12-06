﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bitig.Data.Model;
using Bitig.Logic.Model;
using Bitig.Logic.Repository;

namespace Bitig.Data.Storage
{
    public class XmlContext : IDataContext
    {
        private InMemoryList<XmlAlifba> alifbaCache;
        private InMemoryList<XmlDirection> directionCache;

        private XmlAlifbaReader xmlAlifbaReader;
        private XmlDirectionReader xmlDirectionReader;

        private XmlAlifbaRepository alifbaRepository;
        private XmlDirectionRepository directionRepository;

        public bool IsFlushable { get { return true; } }

        internal InMemoryList<XmlAlifba> Alphabets
        {
            get
            {
                if (alifbaCache == null)
                    InitAlifbaCache();
                return alifbaCache;
            }
        }


        internal InMemoryList<XmlDirection> Directions
        {
            get
            {
                if (directionCache == null)
                    InitDirectionCache();
                return directionCache;
            }
        }

        public AlifbaRepository AlifbaRepository
        {
            get
            {
                return alifbaRepository;
            }
        }

        public DirectionRepository DirectionRepository
        {
            get
            {
                return directionRepository;
            }
        }

        public XmlContext(string DirectoryPath) 
            :this(Path.Combine(DirectoryPath, "Alphabets.xml"), Path.Combine(DirectoryPath, "Directions.xml"))
        {
        }

        public XmlContext(string AlphabetsPath, string DirectionsPath)
        {
            xmlAlifbaReader = new XmlAlifbaReader(AlphabetsPath);
            alifbaRepository = new XmlAlifbaRepository(this);
            xmlDirectionReader = new XmlDirectionReader(DirectionsPath);
            directionRepository = new XmlDirectionRepository(this);
        }

        private void InitAlifbaCache()
        {
            var _xmlList = xmlAlifbaReader.Read();
            if (_xmlList == null)
            {
                _xmlList = new List<XmlAlifba>();
                var _IDs = new List<int>();
                foreach (var _builtIn in DefaultConfiguration.BuiltInAlifbaList)
                {
                    var _id = IDGenerator.GenerateID(_IDs);
                    _IDs.Add(_id);
                    var _alifba = new Alifba(_id, _builtIn.DefaultName, _builtIn.CustomSymbols,
                        _builtIn.RightToLeft, _builtIn.DefaultFont, _builtIn.ID);
                    _xmlList.Add(new XmlAlifba(_alifba));
                }
                xmlAlifbaReader.Save(_xmlList);
            }
            else
            {
                if (!_xmlList.Any(_alif => _alif.BuiltIn == BuiltInAlifbaType.Yanalif))
                {
                    var _id = IDGenerator.GenerateID(_xmlList.Select(_item => _item.ID));
                    var _yanalif = new Alifba(_id, DefaultConfiguration.Yanalif.DefaultName, DefaultConfiguration.Yanalif.CustomSymbols,
                    DefaultConfiguration.Yanalif.RightToLeft, DefaultConfiguration.Yanalif.DefaultFont, DefaultConfiguration.Yanalif.ID);
                    _xmlList.Add(new XmlAlifba(_yanalif));
                    xmlAlifbaReader.Save(_xmlList);
                }
            }
            alifbaCache = new InMemoryList<XmlAlifba>(_xmlList);
        }
        
        private void InitDirectionCache()
        {
            if (alifbaCache == null)
                InitAlifbaCache();
            var _xmlList = xmlDirectionReader.Read();
            if (_xmlList == null)
            {
                _xmlList = new List<XmlDirection>();
                int _count = DefaultConfiguration.BuiltInDirections.Count;
                for (int i = 0; i < _count; i++)
                {
                    var _builtIn = DefaultConfiguration.BuiltInDirections[i];
                    var _alifbaList = alifbaCache.GetList();
                    var _source = _alifbaList.Find(x => x.BuiltIn == _builtIn.Source);
                    var _target = _alifbaList.Find(x => x.BuiltIn == _builtIn.Target);
                    if (_source != null && _target != null)
                    {
                        var _direction = new XmlDirection(i, _source.ID, _target.ID,null, BuiltInID: _builtIn.ID);
                        _xmlList.Add(_direction);
                    }
                }
                xmlDirectionReader.Save(_xmlList);
            }
            directionCache = new InMemoryList<XmlDirection>(_xmlList);
        }

        public void CancelChanges()
        {
            alifbaCache = null;
            directionCache = null;
        }

        public void SaveChanges()
        {
            if (alifbaCache != null)
                xmlAlifbaReader.Save(alifbaCache);
            if (directionCache != null)
                xmlDirectionReader.Save(directionCache);
        }
    }
}
