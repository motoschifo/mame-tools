#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using MameTools.Net48.Extensions;
using MameTools.Net48.Helpers;
using MameTools.Net48.Resources;
using MameTools.Net48.Software;
using MameTools.Net48.Software.Info;
using MameTools.Net48.Software.Parts;
using MameTools.Net48.Software.Parts.DataAreas;
using MameTools.Net48.Software.Parts.DataAreas.Roms;
using MameTools.Net48.Software.Parts.DipSwitches;
using MameTools.Net48.Software.Parts.DiskAreas;
using MameTools.Net48.Software.Parts.Disks;
using MameTools.Net48.Software.Parts.Features;
using MameTools.Net48.Software.SharedFeatures;
using MameTools.Net48.SoftwareList;
namespace MameTools.Net48.Imports;

public static class ImportSoftware
{
    public static async Task LoadFromFile(Mame mame, string filename, Action<string?>? progressUpdate = null, string? prefix = "",
        int loadNodes = MameSoftwareNodes.Defaults, CancellationToken cancellationToken = default)
    {
        mame.SoftwareLists.Clear();
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(filename) || !File.Exists(filename)) return;
        if (string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(mame.ReleaseNumber)) prefix = $"[{mame.ReleaseNumber}] ";

        var i = 0;
        //CacheManager<MameSoftwareListCollection>? cache = null;
        //if (useCache)
        //{
        //    progressUpdate?.Invoke($"{prefix} Lettura file xml software dalla cache");
        //    cache = new(cachePath!);
        //    var list = cache.GetCachedFile(cacheType!, new FileInfo(filename).Name);
        //    if (list is not null)
        //    {
        //        foreach (var sl in list)
        //        {
        //            i++;
        //            if (i % 100 == 0)
        //                progressUpdate?.Invoke(prefix + string.Format("Lettura file xml software dalla cache [{0}] - {1} - {2}", i.ToString("#,##0"), sl.Name, sl.Description));
        //            mame.AddSoftwareList(sl.Name, sl.Description);
        //            foreach (var s in sl.Software)
        //            {
        //                mame.AddSoftware(sl.Name, s);
        //            }
        //        }
        //        return;
        //    }
        //}

        progressUpdate?.Invoke($"{prefix}{Strings.SoftwareFileLoading}");
        using XmlTextReader xml = new(filename)
        {
            WhitespaceHandling = WhitespaceHandling.None
        };
        _ = xml.MoveToContent();

        MameSoftwareList? sl = null;
        //var stopwatch = Stopwatch.StartNew();

        if (!xml.IsEmptyElement)
        {
            do
            {
                i++;
                if (i % 1000 == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progressUpdate?.Invoke($"{prefix}{Strings.SoftwareFileLoading} [{i:#,##0}] - {sl?.Name} - {sl?.Description}");
                }
                if (!xml.IsStartElement())
                    continue;
                if ("softwarelist".EqualsIgnoreCase(xml.LocalName))
                {
                    // Inizio di un nodo "softwarelist"
                    sl = ProcessNodeSoftwareList(xml, loadNodes);
                    mame.SoftwareLists.Add(sl);
                }
            } while (xml.Read());
        }
        cancellationToken.ThrowIfCancellationRequested();
        xml.Close();
        //stopwatch.Stop();
        //Console.WriteLine($"{mame.SoftwareLists.Count} nodes read ({stopwatch.ElapsedMilliseconds} ms)");

        //if (useCache)
        //{
        //    progressUpdate?.Invoke($"{prefix} Scrittura file xml software nella cache");
        //    cache!.SetCacheFile(cacheType!, new FileInfo(filename).Name, mame.SoftwareLists);
        //}
        await Task.CompletedTask;
    }

    private static MameSoftwareList ProcessNodeSoftwareList(XmlReader xml, int loadNodes)
    {
        var nodeName = xml.LocalName;
        var sl = new MameSoftwareList
        {
            Name = xml.GetAttribute("name"),
            Description = xml.GetAttribute("description")
        };

        xml.ProcessNode(reader =>
        {
            if ("notes".EqualsIgnoreCase(reader.LocalName))
            {
                // Legge il contenuto testuale del nodo <notes>
                _ = reader.Read();
                sl.Notes = reader.Value;
            }
            else if ("software".EqualsIgnoreCase(reader.LocalName))
            {
                sl.Software.Add(ProcessNodeSoftware(reader, loadNodes));
            }
        });

        return sl;
    }

    private static MameSoftware ProcessNodeSoftware(XmlReader xml, int loadNodes)
    {
        var nodeName = xml.LocalName;
        var software = new MameSoftware
        {
            Name = xml.GetAttribute("name"),
            CloneOf = xml.GetAttribute("cloneof"),
            Supported = MameSoftware.ParseSupported(xml.GetAttribute("supported"))
        };

        xml.ProcessNode(reader =>
        {
            if ("description".EqualsIgnoreCase(reader.LocalName))
            {
                _ = reader.Read();
                software.Description = reader.Value;
            }
            else if ("year".EqualsIgnoreCase(reader.LocalName))
            {
                _ = reader.Read();
                software.Year = reader.Value;
            }
            else if ("publisher".EqualsIgnoreCase(reader.LocalName))
            {
                _ = reader.Read();
                software.Publisher = reader.Value;
            }
            else if ("notes".EqualsIgnoreCase(reader.LocalName))
            {
                _ = reader.Read();
                software.Notes = reader.Value;
            }
            else if ("info".EqualsIgnoreCase(reader.LocalName))
            {
                if ((loadNodes & MameSoftwareNodes.Info) == 0)
                    return;
                software.Info.Add(ProcessNodeInfo(reader));
            }
            else if ("sharedfeat".EqualsIgnoreCase(reader.LocalName))
            {
                if ((loadNodes & MameSoftwareNodes.SharedFeature) == 0)
                    return;
                software.SharedFeatures.Add(ProcessNodeSharedFeature(reader));
            }
            else if ("part".EqualsIgnoreCase(reader.LocalName))
            {
                if ((loadNodes & MameSoftwareNodes.Part) == 0)
                    return;
                software.Parts.Add(ProcessNodePart(reader, loadNodes));
            }
        });
        return software;
    }

    private static Info ProcessNodeInfo(XmlReader xml)
    {
        return new Info
        {
            Name = xml.GetAttribute("name"),
            Value = xml.GetAttribute("value")
        };
    }

    private static SharedFeature ProcessNodeSharedFeature(XmlReader xml)
    {
        return new SharedFeature
        {
            Name = xml.GetAttribute("name"),
            Value = xml.GetAttribute("value")
        };
    }

    private static Part ProcessNodePart(XmlReader xml, int loadNodes)
    {
        var nodeName = xml.LocalName;
        var part = new Part
        {
            Name = xml.GetAttribute("name"),
            Interface = xml.GetAttribute("interface")
        };
        xml.ProcessNode(reader =>
        {
            if ("feature".EqualsIgnoreCase(reader.LocalName))
            {
                if ((loadNodes & MameSoftwareNodes.Part_Feature) == 0)
                    return;
                part.Features.Add(ProcessNodeFeature(reader));
            }
            else if ("dataarea".EqualsIgnoreCase(reader.LocalName))
            {
                if ((loadNodes & MameSoftwareNodes.Part_DataArea) == 0)
                    return;
                part.DataAreas.Add(ProcessNodeDataArea(reader));
            }
            else if ("diskarea".EqualsIgnoreCase(reader.LocalName))
            {
                if ((loadNodes & MameSoftwareNodes.Part_DiskArea) == 0)
                    return;
                part.DiskAreas.Add(ProcessNodeDiskArea(reader));
            }
            else if ("dipswitch".EqualsIgnoreCase(reader.LocalName))
            {
                if ((loadNodes & MameSoftwareNodes.Part_DipSwitch) == 0)
                    return;
                part.DipSwitches.Add(ProcessNodeDipSwitch(reader));
            }
        });
        return part;
    }

    private static Feature ProcessNodeFeature(XmlReader xml)
    {
        return new Feature
        {
            Name = xml.GetAttribute("name"),
            Value = xml.GetAttribute("value")
        };
    }

    private static DataArea ProcessNodeDataArea(XmlReader xml)
    {
        var nodeName = xml.LocalName;
        var dataArea = new DataArea
        {
            Name = xml.GetAttribute("name"),
            Size = xml.GetAttribute<int>("size") ?? 0,
            DataBits = xml.GetAttribute<int>("databits") ?? 8,
            Endian = DataArea.ParseEndian(xml.GetAttribute("endian"))
        };

        xml.ProcessNode(reader =>
        {
            if ("rom".EqualsIgnoreCase(reader.LocalName))
            {
                var rom = new Rom
                {
                    Name = reader.GetAttribute("name"),
                    Size = reader.GetAttribute<int>("size") ?? 0,
                    Length = reader.GetAttribute<int>("length") ?? 0,
                    CRC = reader.GetAttribute("crc"),
                    SHA1 = reader.GetAttribute("sha1"),
                    Offset = reader.GetAttribute("offset"),
                    Value = reader.GetAttribute("value"),
                    LoadFlag = Rom.ParseLoadFlag(reader.GetAttribute("loadflag")),
                    Status = Rom.ParseStatus(reader.GetAttribute("status"))
                };
                dataArea.Roms.Add(rom);
            }
        });

        return dataArea;
    }
    private static DiskArea ProcessNodeDiskArea(XmlReader xml)
    {
        var nodeName = xml.LocalName;
        var diskArea = new DiskArea
        {
            Name = xml.GetAttribute("name")
        };
        xml.ProcessNode(reader =>
        {
            if ("disk".EqualsIgnoreCase(reader.LocalName))
            {
                var disk = new Disk
                {
                    Name = reader.GetAttribute("name"),
                    SHA1 = reader.GetAttribute("sha1"),
                    Status = Disk.ParseStatus(reader.GetAttribute("status")),
                    Writeable = "yes".EqualsIgnoreCase(reader.GetAttribute("writeable"))
                };
                diskArea.Disks.Add(disk);
            }
        });
        return diskArea;
    }

    private static DipSwitch ProcessNodeDipSwitch(XmlReader xml)
    {
        var nodeName = xml.LocalName;
        var dipSwitch = new DipSwitch
        {
            Name = xml.GetAttribute("name"),
            Tag = xml.GetAttribute("tag"),
            Mask = xml.GetAttribute("mask")
        };
        xml.ProcessNode(reader =>
        {
            if ("dipvalue".EqualsIgnoreCase(reader.LocalName))
            {
                var dipValue = new DipValue
                {
                    Name = reader.GetAttribute("name"),
                    Value = reader.GetAttribute("value"),
                    Default = "yes".EqualsIgnoreCase(reader.GetAttribute("default"))
                };
                dipSwitch.DipValues.Add(dipValue);
            }
        });
        return dipSwitch;
    }


    public static async Task LoadFromHashFolder(Mame mame, string folder, Action<string?>? progressUpdate = null, string? prefix = "",
        int loadNodes = MameSoftwareNodes.Defaults, CancellationToken cancellationToken = default)
    {
        mame.SoftwareListHashes.Clear();
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return;
        if (string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(mame.ReleaseNumber)) prefix = $"[{mame.ReleaseNumber}] ";

        var i = 0;
        //CacheManager<MameSoftwareListCollection>? cache = null;
        //if (useCache)
        //{
        //    progressUpdate?.Invoke($"{prefix} Lettura file xml software hash dalla cache");
        //    cache = new(cachePath!);
        //    var list = cache.GetCachedFile(cacheType!, new FileInfo(folder).Name);
        //    if (list is not null)
        //    {
        //        foreach (var sl in list)
        //        {
        //            i++;
        //            if (i % 100 == 0)
        //                progressUpdate?.Invoke(prefix + string.Format("Lettura file xml software hash dalla cache [{0}] - {1} - {2}", i.ToString("#,##0"), sl.Name, sl.Description));
        //            mame.AddSoftwareListFromHash(sl.Name, sl.Description);
        //            foreach (var s in sl.Software)
        //            {
        //                mame.AddSoftwareFromHash(sl.Name, s);
        //            }
        //        }
        //        return;
        //    }
        //}

        progressUpdate?.Invoke($"{prefix}{Strings.SoftwareHashFileLoading}");

        foreach (var file in Directory.GetFiles(folder, "*.xml", SearchOption.TopDirectoryOnly))
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fiHash = new FileInfo(file);
                progressUpdate?.Invoke($"{prefix}{Strings.SoftwareHashFileLoading} {fiHash.Name}");

                var filename = Path.Combine(folder, fiHash.Name);
                using XmlTextReader xml = new(filename)
                {
                    WhitespaceHandling = WhitespaceHandling.None
                };
                _ = xml.MoveToContent();

                MameSoftwareList? sl = null;
                var stopwatch = Stopwatch.StartNew();

                if (!xml.IsEmptyElement)
                {
                    do
                    {
                        i++;
                        if (i % 1000 == 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            progressUpdate?.Invoke($"{prefix}{Strings.SoftwareFileLoading} [{i:#,##0}] - {sl?.Name} - {sl?.Description}");
                        }
                        if (!xml.IsStartElement())
                            continue;
                        if ("softwarelist".EqualsIgnoreCase(xml.LocalName))
                        {
                            // Inizio di un nodo "softwarelist"
                            sl = ProcessNodeSoftwareList(xml, loadNodes);
                            mame.SoftwareListHashes.Add(sl);
                        }
                    } while (xml.Read());
                }
                cancellationToken.ThrowIfCancellationRequested();
                xml.Close();
                stopwatch.Stop();
                Console.WriteLine($"{mame.SoftwareListHashes.Count} nodi letti ({stopwatch.ElapsedMilliseconds} ms)");
            }
            catch (Exception ex)
            {
                throw new Exception($"File {Path.Combine(folder, file)}: {ex.Message}", ex);
            }
        }
        //if (useCache)
        //{
        //    progressUpdate?.Invoke($"{prefix} Scrittura file xml software hash nella cache");
        //    cache!.SetCacheFile(cacheType!, new FileInfo(folder).Name, mame.SoftwareListHashes);
        //}
        await Task.CompletedTask;
    }

}
