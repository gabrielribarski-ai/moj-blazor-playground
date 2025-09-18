using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Formatters;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Reflection.PortableExecutable;
using static System.Runtime.InteropServices.JavaScript.JSType;
using CustomTypeExtensions;

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
        string q = "SELECT ATRIBUTID, SEGMENTID, STANDARDNAVREDNOST, ENOTA, TIPMERITVE, MOZNAPRIMERJAVA, IME from B_ATRIBUTI ";

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
                                AtributId = dr["ATRIBUTID"].ToString(),
                                Ime= dr["IME"].ToString(),
                                SegmentId = dr["SEGMENTID"].ToString(),
                                StandardnaVrednost = dr["STANDARDNAVREDNOST"]?.ToDecimal(),
                                TipMeritve = dr["TIPMERITVE"].ToString(),
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
        string q = " SELECT seg.SEGMENTID, seg.IME, seg.NADREJENIID, seg.TIP, seg.STRANSKOST, ";
        q += "( select count (atr.atributid) from b_atributi atr   where atr.segmentid=seg.segmentid    ) as st_kriterijev ";
        q += "FROM B_SEGMENTI seg";


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
                    SegmentId = dr["SEGMENTID"].ToString(),
                    Ime = dr["IME"].ToString(),
                    
                    NadsegmentId = (dr["NADREJENIID"].ToString() == "" ? null : dr["NADREJENIID"].ToString()),
                    //Tip = reader.GetString(3),
                    Stranskost = dr["STRANSKOST"].ToString(),
                    Atributi = new List<Atribut>(),
                    Children = new List<Segment>(),
                    ImaOcenjevalneAtribute= dr["st_kriterijev"].ToString()!="0",
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
        return Seznam.FirstOrDefault(s => s.Ime.Equals(ime, StringComparison.OrdinalIgnoreCase));
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


    public List<Segment> GetSegmentsByStranskost(string stranskost)
    {
        return Seznam
            .Where(s => s.Stranskost.Equals(stranskost, StringComparison.OrdinalIgnoreCase)
                     || s.Stranskost.Equals("oboje", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }


}