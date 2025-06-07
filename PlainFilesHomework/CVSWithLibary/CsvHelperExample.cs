using CsvHelper;
using System.Formats.Asn1;
using System;
using System.Globalization;

namespace CVSWithLibary;

public class CsvHelperExample
{
    public void Write(string path, IEnumerable<Person> people)
    {
        using var sw = new StreamWriter(path);
        using var cw = new CsvWriter(sw, CultureInfo.InvariantCulture);
        cw.WriteRecords(people);
    }

    public IEnumerable<Person> Read(string path)
    {
        if (!File.Exists(path))
        {
            return Enumerable.Empty<Person>();
        }
        using var sr = new StreamReader(path);
        using var cr = new CsvReader(sr, CultureInfo.InvariantCulture);
        return cr.GetRecords<Person>().ToList();
    }
}
