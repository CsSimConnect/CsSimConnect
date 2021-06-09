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
        None = 0x0,
        Copyright = 0x1,
        Guid = 0x2,
        Airport = 0x3,
        IlsVor = 0x13,
        Ndb = 0x17,
        Marker = 0x18,
        Boundary = 0x20,
        Waypoint = 0x22,
        Geopol = 0x23,
        SceneryObject = 0x25,
        NameList = 0x27,
        VorIlsIcaoIndex = 0x28,
        NdbIcaoIndex = 0x29,
        WaypointIcaoIndex = 0x2A,
        ModelData = 0x2B,
        AirportSummary = 0x2C,
        Exclusion = 0x2E,
        TimeZone = 0x2F,
        TerrainVectorDb = 0x65,
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
        public readonly List<BglSubSection> subSections = new();

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

        internal BglSection(BglFile file, uint index)
        {
            this.file = file;
            Index = index;
            using var sectionReader = file.File.Section(file.SectionHeaderOffset(index), BglSectionHeader.Size);
            sectionReader.Read(out header, BglSectionHeader.Size);

            for (uint subIndex = 0; subIndex < header.SubSectionCount; subIndex++)
            {
                subSections.Add(new(this, subIndex));
            }
        }

        internal uint SubSectionHeaderOffset(uint index)
        {
            return header.SubSectionStartOffset + (index * header.SubSectionSize);
        }

        public bool HaveSubsections => subSections.Count != 0;
        public BglAirport Airport => HaveSubsections ? subSections[0].Airport : null;
    }
}
