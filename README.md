# Week 2: Arrays and Complex CSV Parsing

> **Template Purpose:** This template represents a working solution through Week 1. Use YOUR repo if you're caught up. Use this as a fresh start if needed.

---

## Overview

This week you'll expand your console application to handle more complex data structures. You'll learn to parse CSV files with quoted strings, commas inside values, and equipment arrays. These parsing skills prepare you for handling real-world data formats.

## Learning Objectives

By completing this assignment, you will:
- [ ] Handle arrays (equipment lists) in CSV data
- [ ] Parse quoted strings containing commas
- [ ] Work with header rows in CSV files
- [ ] Use string methods like `IndexOf`, `Substring`, and `Trim`

## Prerequisites

Before starting, ensure you have:
- [ ] Completed Week 1 assignment (or are using this template)
- [ ] Working file read/write operations
- [ ] Basic menu structure in place

## What's New This Week

| Concept | Description |
|---------|-------------|
| `String.IndexOf()` | Find position of a character in a string |
| `String.Substring()` | Extract a portion of a string |
| `String.Trim()` | Remove characters from start/end of string |
| Equipment Arrays | Store multiple items per character (e.g., `sword|shield|potion`) |

---

## Assignment Tasks

### Task 1: Handle Header Rows

**What to do:**
- Modify your CSV reading to skip or handle a header row
- First line of CSV might be: `Name,Class,Level,HP,Equipment`

**Example:**
```csharp
string[] lines = File.ReadAllLines("input.csv");
// Skip header row (index 0)
for (int i = 1; i < lines.Length; i++)
{
    // Process data rows
}
```

### Task 2: Parse Quoted Names

**What to do:**
- Handle names like `"Smith, John"` where the comma is part of the name
- Detect when a field starts with a quote
- Find the closing quote before splitting

**Example:**
```csharp
if (line.StartsWith("\""))
{
    int closingQuote = line.IndexOf("\"", 1);
    string name = line.Substring(1, closingQuote - 1);
    // Continue parsing after the quoted section
}
```

### Task 3: Handle Equipment Arrays

**What to do:**
- Equipment is stored as pipe-separated values: `sword|shield|potion`
- Parse into a string array
- Display equipment as a list
- Allow adding/removing equipment

**Example:**
```csharp
string equipmentField = "sword|shield|potion";
string[] equipment = equipmentField.Split('|');
// equipment[0] = "sword", equipment[1] = "shield", etc.
```

### Task 4: Write Back Correctly

**What to do:**
- When saving, preserve the CSV format
- Re-quote names that contain commas
- Join equipment arrays back with pipe separator

---

## Stretch Goal (+10%)

**Use the CsvHelper Library**

CsvHelper handles all these edge cases automatically. Install it and refactor your code:

```bash
dotnet add package CsvHelper
```

```csharp
using CsvHelper;
using System.Globalization;

using var reader = new StreamReader("input.csv");
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
var records = csv.GetRecords<Character>().ToList();
```

---

## Menu Structure

Your menu should still look like:
```
1. Display Characters
2. Add Character
3. Level Up Character
0. Exit
```

---

## Sample CSV Format

```csv
Name,Class,Level,HP,Equipment
"Smith, John",Warrior,5,100,sword|shield|potion
Jane,Mage,3,60,staff|robe|scroll
```

---

## Grading Rubric

| Criteria | Points | Description |
|----------|--------|-------------|
| File I/O Operations | 25 | Reads and writes CSV with correct formatting |
| Array Handling | 25 | Correctly parses and saves equipment arrays |
| String Parsing | 25 | Handles quoted names and commas properly |
| Program Flow | 15 | Menu works, no crashes |
| Code Quality | 10 | Clean, readable, well-commented |
| **Total** | **100** | |
| **Stretch: CsvHelper** | **+10** | Successfully uses CsvHelper library |

---

## How This Connects to the Final Project

- Equipment arrays will become your Inventory system
- Parsing skills help when working with JSON in Week 4
- The Character model continues to grow each week
- Understanding data formats prepares you for database work

---

## Tips

- Test with edge cases: empty equipment, names with commas, special characters
- Use `Console.WriteLine()` to debug your parsing step by step
- The pipe `|` character is a good delimiter because it rarely appears in game data
- Consider creating a `Character` class to hold parsed data (preview of Week 3)

---

## Submission

1. Commit your changes with a meaningful message
2. Push to your GitHub Classroom repository
3. Submit the repository URL in Canvas

---

## Resources

- [String.IndexOf Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.string.indexof)
- [String.Substring Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.string.substring)
- [CsvHelper Documentation](https://joshclose.github.io/CsvHelper/)

---

## Need Help?

- Post questions in the Canvas discussion board
- Attend office hours
- Review the in-class repository for additional examples