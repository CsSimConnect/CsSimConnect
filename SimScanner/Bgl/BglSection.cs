/*
 * Copyright (c) 2021. Bert Laverman
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;

namespace SimScanner.Bgl
{
    public enum SectionType
    {
        None = 0x00,
        Copyright = 0x01,
        Guid = 0x02,
        Airport = 0x03,
        RunWay = 0x04,
        RunwayStart = 0x11,
        Com = 0x12,
        IlsVor = 0x13,
        Ndb = 0x17,
        Marker = 0x18,
        AirportName = 0x19,
        TaxiwayPoint = 0x1a,
        TaxiwayPath = 0x1c,
        TaxiwayName = 0x1d,
        Boundary = 0x20,
        Waypoint = 0x22,
        Geopol = 0x23,
        Approach = 0x24,
        SceneryObject = 0x25,
        Helipad = 0x26,
        NameList = 0x27,
        VorIlsIcaoIndex = 0x28,
        NdbIcaoIndex = 0x29,
        WaypointIcaoIndex = 0x2A,
        ModelData = 0x2B,
        AirportSummary = 0x2C,
        Exclusion = 0x2E,
        TimeZone = 0x2F,
        ApronDetail = 0x30,
        ApronEdgeLight = 0x31,
        DeleteAirport = 0x33,
        ApronSurface = 0x37,
        BlastFence = 0x38,
        BoundaryFence = 0x39,
        Jetway = 0x3A,
        Unknown3B = 0x3B,
        TaxiwayParking = 0x3d,
        TerrainVectorDb = 0x65,
        AirportTowerScenery = 0x66,
        TerrainElevation = 0x67,
        TerrainLandClass = 0x68,
        TerrainWaterClass = 0x69,
        TerrainRegion = 0x6A,
        PopulationDensity = 0x6C,
        AutogenAnnotation = 0x6D,
        TerrainIndex = 0x6E,
        TerrainTextureLookup = 0x6F,
        TerrainSeasonJan = 0x78,
        TerrainSeasonFeb = 0x79,
        TerrainSeasonMar = 0x7A,
        TerrainSeasonApr = 0x7B,
        TerrainSeasonMay = 0x7C,
        TerrainSeasonJun = 0x7D,
        TerrainSeasonJul = 0x7E,
        TerrainSeasonAug = 0x7F,
        TerrainSeasonSep = 0x80,
        TerrainSeasonOct = 0x81,
        TerrainSeasonNov = 0x82,
        TerrainSeasonDec = 0x83,
        TerrainPhotoJan = 0x8C,
        TerrainPhotoFeb = 0x8D,
        TerrainPhotoMar = 0x8E,
        TerrainPhotoApr = 0x8F,
        TerrainPhotoMay = 0x90,
        TerrainPhotoJun = 0x91,
        TerrainPhotoJul = 0x92,
        TerrainPhotoAug = 0x93,
        TerrainPhotoSep = 0x94,
        TerrainPhotoOct = 0x95,
        TerrainPhotoNov = 0x96,
        TerrainPhotoDec = 0x97,
        TerrainPhotoNight = 0x98,
        Tacan = 0xA0,
        TacanIndex = 0xA1,
        UnknownAA = 0xAA,
        FakeTypes = 0x2710,
        IcaoRunway = 0x2711
    }

    public struct BglSectionHeader
    {
        public const uint Size = 20;

        public readonly uint EncodedType;
        public readonly uint EncodedSize;
        public readonly uint SubSectionCount;
        public readonly uint SubSectionStartOffset;
        public readonly uint SubSectionTotalSize;

        public SectionType Type => (SectionType)Enum.ToObject(typeof(SectionType), EncodedType);
        public uint SubSectionSize => ((EncodedSize & 0x10000) | 0x40000) >> 0x0e;
        public bool Valid => SubSectionTotalSize == (SubSectionSize * SubSectionCount);
    }

    public class BglSection
    {
        internal BglFile file;

        private BglSectionHeader header;
        public BglSectionHeader Header => header;
        public uint Index { get; init; }
        public readonly Dictionary<uint, BglSubSection> subSections = new();

        public SectionType Type => header.Type;
        public uint SubSectionCount => header.SubSectionCount;
        public uint SubSectionSize => header.SubSectionSize;

        private readonly HashSet<SectionType> terrainTypes = new() {
            SectionType.Exclusion,
            SectionType.TerrainVectorDb,
            SectionType.TerrainElevation,
            SectionType.TerrainLandClass,
            SectionType.TerrainWaterClass,
            SectionType.TerrainRegion,
            SectionType.TerrainIndex,
            SectionType.TerrainTextureLookup,
            SectionType.TerrainSeasonJan,
            SectionType.TerrainSeasonFeb,
            SectionType.TerrainSeasonMar,
            SectionType.TerrainSeasonApr,
            SectionType.TerrainSeasonMay,
            SectionType.TerrainSeasonJun,
            SectionType.TerrainSeasonJul,
            SectionType.TerrainSeasonAug,
            SectionType.TerrainSeasonSep,
            SectionType.TerrainSeasonOct,
            SectionType.TerrainSeasonNov,
            SectionType.TerrainSeasonDec,
            SectionType.TerrainPhotoJan,
            SectionType.TerrainPhotoFeb,
            SectionType.TerrainPhotoMar,
            SectionType.TerrainPhotoApr,
            SectionType.TerrainPhotoMay,
            SectionType.TerrainPhotoJun,
            SectionType.TerrainPhotoJul,
            SectionType.TerrainPhotoAug,
            SectionType.TerrainPhotoSep,
            SectionType.TerrainPhotoOct,
            SectionType.TerrainPhotoNov,
            SectionType.TerrainPhotoDec,
            SectionType.TerrainPhotoNight,
        };

        public bool IsAirport => Type == SectionType.Airport;
        public bool IsIlsVor => Type == SectionType.IlsVor;
        public bool IsNdb => Type == SectionType.Ndb;
        public bool IsMarker => Type == SectionType.Marker;
        public bool IsWaypoint => Type == SectionType.Waypoint;
        public bool IsSceneryObject => Type == SectionType.SceneryObject;
        public bool IsTerrain => terrainTypes.Contains(Type);
        public bool IsNameList => Type == SectionType.NameList;

        internal BglSection(BglFile file, uint index)
        {
            this.file = file;
            Index = index;
            using var sectionReader = file.File.Section(file.SectionHeaderOffset(index), BglSectionHeader.Size);
            sectionReader.Read(out header, BglSectionHeader.Size);
        }

        internal uint SubSectionHeaderOffset(uint index)
        {
            return header.SubSectionStartOffset + (index * header.SubSectionSize);
        }

        public bool HaveSubsections => SubSectionCount != 0;

        public BglSubSection GetSubSection(uint index)
        {
            if (!HaveSubsections || index > SubSectionCount)
            {
                return null;
            }
            if (!subSections.ContainsKey(index))
            {
                subSections.Add(index, new BglSubSection(this, index));
            }
            return subSections[index];
        }

        public IEnumerable<BglSubSection> EnumerateSubSections()
        {
            for (uint i = 0; i < header.SubSectionCount; i++)
            {
                yield return GetSubSection(i);
            }
        }

        public IEnumerable<BglSubSection> SubSections => EnumerateSubSections();

        public BglAirport GetAirport(uint subSectionNr)
        {
            return subSectionNr < header.SubSectionCount ? GetSubSection(subSectionNr)?.Airport : null;
        }

        public IEnumerable<BglAirport> EnumerateAirports()
        {
            if (IsAirport)
            {
                for (uint i = 0; i < header.SubSectionCount; i++)
                {
                    yield return GetAirport(i);
                }
            }
        }

        public IEnumerable<BglAirport> Airports => EnumerateAirports();

        public BglNameList GetNameList(uint subSectionNr)
        {
            return subSectionNr < header.SubSectionCount ? GetSubSection(subSectionNr)?.NameList : null;
        }

        public IEnumerable<BglNameList> EnumerateNameLists()
        {
            if (IsNameList)
            {
                for (uint i = 0; i < header.SubSectionCount; i++)
                {
                    yield return GetNameList(i);
                }
            }
        }
        public IEnumerable<BglNameList> NameLists => EnumerateNameLists();
    }
}
