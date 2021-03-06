﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Bitig.Data.Storage;
using Bitig.Logic.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitigDataTests
{
    [TestClass]
    public class ExclusionsTests
    {
        private const string dataFolder = @"TestData\";
        private const string currentDataFolder = @"ExclusionsTestData\";
        private readonly string preparedDirectionsFile = currentDataFolder + @"DirectionCyrYan.xml";
        private readonly string preparedAlphabetsFile = currentDataFolder + @"Alphabet1025.xml";

        [TestInitialize]
        [TestCleanup]
        public void DeleteTestFile()
        {
            if (Directory.Exists(currentDataFolder))
                Directory.Delete(currentDataFolder, true);
            Directory.CreateDirectory(currentDataFolder);
            File.Copy(dataFolder + @"DirectionCyrYan.xml", preparedDirectionsFile);
            File.Copy(dataFolder + "Alphabet1025.xml", preparedAlphabetsFile);
            TestUtils.CopyDirectory(dataFolder + @"Exclusions", currentDataFolder + "Exclusions");
        }

        [TestMethod]
        public void AddExclusion()
        {
            var _dirRepo = new XmlContext(preparedAlphabetsFile, preparedDirectionsFile).DirectionRepository;
            var _direction = _dirRepo.Get(0);
            var _source = GenerateCyrillicWord();
            var _target = Guid.NewGuid().ToString();
            _direction.Exclusions.Add(new Bitig.Logic.Model.Exclusion { SourceWord = _source, TargetWord = _target });
            _dirRepo.Update(_direction);
            _dirRepo.DataContext.SaveChanges();
            _direction = _dirRepo.Get(_direction.ID);
            var _result = _direction.Transliterate(_source);
            Assert.AreEqual(_target, _result);
        }

        [TestMethod]
        public void EditExclusion()
        {
            var _dirRepo = new XmlContext(preparedAlphabetsFile, preparedDirectionsFile).DirectionRepository;
            var _direction = _dirRepo.Get(0);
            Assert.AreEqual("ike", _direction.Transliterate("бер"));
            var _target = Guid.NewGuid().ToString();
            _direction.Exclusions[0].TargetWord = _target;
            _dirRepo.Update(_direction);
            _dirRepo.DataContext.SaveChanges();
            _direction = _dirRepo.Get(_direction.ID);
            var _result = _direction.Transliterate("бер");
            Assert.AreEqual(_target, _result);
        }

        [TestMethod]
        public void DeleteThenAdd()
        {
            var _context = new XmlContext(preparedAlphabetsFile, preparedDirectionsFile);
            var _dirRepo = _context.DirectionRepository;
            var _direction = _dirRepo.Get(0);
            Assert.AreEqual(1, _direction.Exclusions.Count);
            _direction.Exclusions = null;
            _dirRepo.Update(_direction);
            _context.SaveChanges();
            _direction = _dirRepo.Get(0);
            var _exclusion = new Exclusion(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), false, false, false);
            _direction.Exclusions = new List<Exclusion> { _exclusion };
            _dirRepo.Update(_direction);
            _context.SaveChanges();

            var _checkDirection = _dirRepo.Get(0);
            Assert.AreEqual(1, _checkDirection.Exclusions.Count);
            Assert.AreEqual(_exclusion.SourceWord, _checkDirection.Exclusions[0].SourceWord);
        }

        private string GenerateCyrillicWord()
        {
            var _symbolDict = new Dictionary<char, char>();
            _symbolDict.Add('0', 'а');
            _symbolDict.Add('1', 'б');
            _symbolDict.Add('2', 'в');
            _symbolDict.Add('3', 'г');
            _symbolDict.Add('4', 'д');
            _symbolDict.Add('5', 'е');
            _symbolDict.Add('6', 'з');
            _symbolDict.Add('7', 'и');
            _symbolDict.Add('8', 'л');
            _symbolDict.Add('9', 'л');
            _symbolDict.Add('a', 'м');
            _symbolDict.Add('b', 'р');
            _symbolDict.Add('c', 'н');
            _symbolDict.Add('d', 'о');
            _symbolDict.Add('e', 'п');
            _symbolDict.Add('f', 'с');
            var _guid = Guid.NewGuid().ToString("N");
            var _result = new StringBuilder();
            foreach (var _char in _guid)
            {
                _result.Append(_symbolDict[_char]);
            }
            return _result.ToString();
        }
    }
}
