// Created 2023-12-04
// Implements logic to load data from a local JSON file (which the user can specify)


using System.Text.Json;
using ASBNApp.Model;


public class LocalASBNDataService : IASBNDataService
{

    // Handling dependency injection
    public LocalASBNDataService(IFileHandleProvider fileHandles, DateHandler dateHandler)
    {
        this.fileHandles = fileHandles;
        this.dateHandler = dateHandler;
    }

    public JSONDataWrapper? DataReadFromJSON;
    private readonly IFileHandleProvider fileHandles;
    private readonly DateHandler dateHandler;

    /// <summary>
    /// Load local file from the available list of file handles, then deserializes
    /// the text into JSON objects
    /// </summary>
    /// <returns>Task</returns>
    /// <exception cref="NullReferenceException">Throw exception if </exception>
    public async Task ReadData()
    {
        var fileHandle = fileHandles.GetFileHandles().Single();
        var file = await fileHandle.GetFileAsync();
        var text = await file.TextAsync();

        // Dynamically load the names of objects and files
        DataReadFromJSON = JsonSerializer.Deserialize<JSONDataWrapper>(text) ??
            throw new NullReferenceException(
                $"The selected file ({await fileHandle.GetNameAsync()}) cannot " +
                $"be casted to an {nameof(JSONDataWrapper)}.");
    }


    // Not implemented yet, might be used to save data to a file
    public void WriteData()
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// Function to get data from the JSON file for a certain date
    /// </summary>
    /// <param name="date">DateTime to load data for</param>
    /// <returns>An EntryRowModel object, either filled with values or empty</returns>
    /// <exception cref="ArgumentNullException">If no date is present, this is thrown</exception>
    public EntryRowModel GetDay(DateTime? date)
    {
        string YearAsString;
        string DateToLoadAsString;
        string WeekNumberAsString;

        if (date.HasValue)
        {
            // Need to run through this because we're using a nullable date
            // Get the parameters we need to get the LoggedData
            YearAsString = date.Value.ToString("yyyy");
            DateToLoadAsString = date.Value.ToString("yyyy-MM-dd");
            WeekNumberAsString = dateHandler.GetWeekOfYear((DateTime)date).ToString();

            // try catch block here to check if we have a key present
            try
            {
                return DataReadFromJSON.LoggedData[YearAsString][WeekNumberAsString][DateToLoadAsString];
            }
            catch (Exception e)
            {
                if (e is NullReferenceException || e is KeyNotFoundException)
                    Console.WriteLine(e.Message +
                        " No data available (an empty EntryRowModel has been returned in the meantime).");
                return new EntryRowModel();
            }
        }
        else
        {
            // if there's no date handed over throw Exception
            throw new ArgumentNullException();
        }
    }

    /// <summary>
    /// Gets an Enumerable with the entries for a week
    /// </summary>
    /// <param name="year">int for the year to get data for</param>
    /// <param name="week">int for the week to get data for</param>
    /// <returns>Enumerable containing the 5 EntryRowModels (filled with data / empty)</returns>
    public IEnumerable<EntryRowModel> GetWeek(int? year, int? week)
    {
        var result = new List<EntryRowModel>();
        try
        {
            // Retrieve data for the selected week & year
            var dataDictionary = DataReadFromJSON.LoggedData[year.ToString()][week.ToString()];

            // iterate over dict & store in a list 
            foreach (var entry in dataDictionary) { result.Add(entry.Value); }
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine(e.Message + " No data loaded, please load JSON file before requesting data.");
        }
        catch (KeyNotFoundException e)
        {
            Console.WriteLine(e.Message + " No data present for this week, returned empty EntryRowModels instead.");
        }

        // Add 5 entries if dataDictionary list is empty (required for the UI)
        if (result.Count == 0)
        {
            if (week != null && year != null)
            {
                var FirstDateInWeek = dateHandler.GetFirstDateOfWeek((int)week, (int)year);
                for (int i = 0; i < 5; i++)
                {
                    result.Add(new EntryRowModel()
                    {
                        Date = FirstDateInWeek.AddDays(i),
                        Location = string.Empty,
                        Note = string.Empty
                    });
                }
            }
        }

        // Return the list of entries as enumerable
        return result.AsEnumerable();
    }


    public Task SaveDay(EntryRowModel entry)
    {
        throw new NotImplementedException();
    }

    public Task SaveWeek(IEnumerable<EntryRowModel> entries)
    {
        throw new NotImplementedException();
    }


    public Settings? GetSettings()
    {
        return DataReadFromJSON.Settings;
    }

    public Task SaveSettings(Settings settings)
    {
        throw new NotImplementedException();
    }

    public List<WorkLocationHours> GetWorkLocationHours()
    {
        try
        {
            var workLocationHours = new List<WorkLocationHours>();
            var workLocationHoursDictionary = DataReadFromJSON.WorkLocationHours;
            foreach (var entry in workLocationHoursDictionary)
            {
                workLocationHours.Add(entry.Value);
            }
            return workLocationHours;
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine(e.Message + " No data available, please load a file first.");
            return new List<WorkLocationHours>();
        }

    }

    public Task SaveWorkLocationHours(WorkLocationHours workLocationHours)
    {
        throw new NotImplementedException();
    }
}