using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Formatters;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Reflection.PortableExecutable;
using static System.Runtime.InteropServices.JavaScript.JSType;
using CustomTypeExtensions;
using IzracunInvalidnostiBlazor.Models;

namespace IzracunInvalidnostiBlazor;

public class SegmentService
{

    private readonly IConfiguration _config;
    public List<Segment>? Seznam;

    public SegmentService(IConfiguration config)
    {
        _config = config;
    }

    public List<Segment> GetAllSegments() => Seznam;

    public async Task PreberiAtributeDB_Sync_Segmenti()
    {
        using var conn = new OracleConnection(_config.GetConnectionString("APL_INVALIDNOST"));
        conn.Open();
        string q = "SELECT ID, SEGMENT_ID, STANDARD, ENOTA, TIP_MERITVE, MOZNA_PRIMERJAVA, OPIS from B1_ATRIBUTI ";

            using (OracleCommand cmd = new OracleCommand(q, conn))
            {
                DataTable dt = new DataTable();
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    dt.Load(reader, LoadOption.PreserveChanges);
                }

                foreach (Segment segment in this.Seznam)
                {
                    if (segment.ImaOcenjevalneAtribute)
                    {
                        IEnumerable<DataRow> atributiRows = dt.AsEnumerable().Select(x => x).Where(x => x["segmentid"].ToString() == segment.SegmentId);
                        segment.Atributi = new();
                        foreach (DataRow dr in atributiRows)
                        {
                            Atribut atr = new Atribut()
                            {
                                AtributId = dr["ID"].ToString(),
                                Opis= dr["OPIS"].ToString(),
                                SegmentId = dr["SEGMENT_ID"].ToString(),
                                StandardnaVrednost = dr["STANDARD"]?.AsDecimal(),
                                TipMeritve = dr["TIP_MERITVE"].ToString() == "NUM" ? TipMeritveEnum.NUM : TipMeritveEnum.BOOL,
                                Enota =dr["ENOTA"].ToString(),
                            };
                            segment.Atributi.Add(atr);
                        }
                    }
                }
            }

    }


    public async Task PreberiSegmenteDB()

    {
        Seznam = new List<Segment>();

        using var conn = new OracleConnection(_config.GetConnectionString("APL_INVALIDNOST"));
        conn.Open();
        string q = "";
        q += "SELECT seg.ID AS SEGMENT_ID, seg.OPIS, seg.NADREJENI_ID, seg.TIP, seg.LDE,";
        q += " (select count(atr.ID) from b1_atributi atr   where atr.segment_id = seg.ID    ) as st_kriterijev";
        q += " FROM B1_SEGMENTI seg";


        using (OracleCommand cmd = new OracleCommand(q, conn))
        {
            DataTable dt = new DataTable();
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                dt.Load(reader, LoadOption.PreserveChanges);
            }
            foreach (DataRow dr in dt.Rows)
            {
                Seznam.Add(new Segment
                {
                    SegmentId = dr["SEGMENT_ID"].ToString(),
                    Opis = dr["OPIS"].ToString(),
                    NadsegmentId = (dr["NADREJENI_ID"].ToString() == "" ? null : dr["NADREJENI_ID"].ToString()),
                    //Tip = reader.GetString(3),
                    SimetrijaDelaTelesa = dr["LDE"].ToString()== "LD" ? SimetrijaDelaTelesa.LD : SimetrijaDelaTelesa.E,
                    Atributi = new List<Atribut>(),
                   // PodSegment = new List<Segment>(),
                    ImaOcenjevalneAtribute = dr["st_kriterijev"].ToString() != "0",
                });
            }
        }
        PreberiAtributeDB_Sync_Segmenti();
    }

    public List<Segment> GetSegmentChildren(string segmentId)
    {
        return Seznam
            .Where(s => s.NadsegmentId == segmentId)
            .ToList();
    }

    public Segment? GetParentSegment(string segmentId)
    {
        var current = Seznam.FirstOrDefault(s => s.SegmentId == segmentId);
        if (current?.NadsegmentId == null) return null;

        return Seznam.FirstOrDefault(s => s.SegmentId == current.NadsegmentId);
    }

    public List<Segment> GetParentsChildren(string segmentId)
    {
        var parent = GetParentSegment(segmentId);
        if (parent == null) return new();

        return GetSegmentChildren(parent.SegmentId);
    }

    public List<Segment> GetBreadcrumbPath(string segmentId)
    {
        var path = new List<Segment>();
        var current = Seznam.FirstOrDefault(s => s.SegmentId == segmentId);

        while (current != null)
        {
            path.Insert(0, current);
            current = GetParentSegment(current.SegmentId);
        }

        return path;
    }


    public Segment? FindSegmentByName(string ime)
    {
        return Seznam.FirstOrDefault(s => s.Opis.Equals(ime, StringComparison.OrdinalIgnoreCase));
    }




    public List<Segment> GetRootSegments()
    {
        return Seznam
            .Where(s => s.NadsegmentId == null)
            .ToList();
    }

    public Segment GetRootSegment()
    {
        return Seznam
            .Where(s => s.NadsegmentId == null)
            .First();
    }

    public List<Segment> GetRootChildren()
    {
        var root = GetRootSegment();
        return Seznam
            .Where(s => s.NadsegmentId == root.SegmentId)
            .ToList();
    }





}