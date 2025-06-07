using CVSWithLibary;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;

var helper = new CsvHelperExample();
var readPeople = helper.Read("people.csv").ToList();

const string usersFilePath = "Users.txt";
List<User> users = LoadUsers(usersFilePath);

Logger.Log("Application started.");

bool authenticated = false;
int loginAttempts = 0;
const int maxLoginAttempts = 3;
User? currentUser = null;

while (!authenticated && loginAttempts < maxLoginAttempts)
{
    Console.WriteLine("=======================================");
    Console.WriteLine("Please log in:");
    Console.Write("Username: ");
    string? username = Console.ReadLine();
    Console.Write("Password: ");
    string? password = ReadPassword();

    currentUser = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

    if (currentUser != null)
    {
        if (!currentUser.IsActive)
        {
            Logger.Log($"Login attempt failed for '{username}': User account locked.");
            Console.WriteLine("Error! Your account is locked. Please contact the administrator.");
            loginAttempts = maxLoginAttempts;
            break;
        }

        if (currentUser.Password == password)
        {
            authenticated = true;
            Logger.SetCurrentUser(currentUser.Username);
            Logger.Log($"Successful login for '{currentUser.Username}'.");
            Console.WriteLine($"Welcome, {currentUser.Username}!");
            Console.WriteLine("=======================================");
        }
        else
        {
            Logger.Log($"Login attempt failed for '{username}': Incorrect password.");
            Console.WriteLine("Error! Incorrect password.");
            loginAttempts++;
        }
    }
    else
    {
        Logger.Log($"Login attempt failed for '{username ?? "NULL_USERNAME"}': User not found.");
        Console.WriteLine("Error! User not found.");
        loginAttempts++;
    }

    if (!authenticated && loginAttempts < maxLoginAttempts)
    {
        Console.WriteLine($"Remaining attempts: {maxLoginAttempts - loginAttempts}");
    }
}

if (!authenticated)
{
    Console.WriteLine("=======================================");
    Console.WriteLine("Too many failed attempts. Exiting program.");
    if (currentUser != null && currentUser.IsActive)
    {
        currentUser.IsActive = false;
        SaveUsers(usersFilePath, users);
        Logger.Log($"User '{currentUser.Username}' has been locked due to multiple failed attempts.");
        Console.WriteLine($"User '{currentUser.Username}' has been locked.");
    }
    else if (currentUser == null)
    {
        Logger.Log("Program terminated due to multiple failed login attempts (user not found).");
    }
    Logger.Log("Application terminated due to authentication failure.");
    Console.WriteLine("=======================================");
    return;
}

Logger.Log("Accessed main menu.");
var opc = "0";
do
{
    opc = Menu();
    Console.WriteLine("=======================================");
    switch (opc)
    {
        case "1":
            Logger.Log("User selected 'Display People Data'.");
            foreach (var person in readPeople)
            {
                Console.WriteLine(person);
            }
            break;

        case "2":
            Logger.Log("User selected 'Add Person'. Starting validations.");

            int newId;
            bool idIsValid = false;
            do
            {
                Console.Write("Enter the ID (must be a unique number): ");
                var idInput = Console.ReadLine();
                if (int.TryParse(idInput, out newId))
                {
                    if (readPeople.Any(p => p.Id == newId))
                    {
                        Console.WriteLine("Error! This ID already exists. Please enter a unique ID.");
                        Logger.Log($"Error adding person: ID '{idInput}' already exists.");
                    }
                    else if (newId < 0)
                    {
                        Console.WriteLine("Error! The ID must be a positive number.");
                        Logger.Log($"Error adding person: ID '{idInput}' is not positive.");
                    }
                    else
                    {
                        idIsValid = true;
                    }
                }
                else
                {
                    Console.WriteLine("Error! The ID must be a valid number.");
                    Logger.Log($"Error adding person: ID '{idInput}' is not a number.");
                }
            } while (!idIsValid);

            string firstName = string.Empty;
            bool firstNameIsValid = false;
            do
            {
                Console.Write("Enter the First Name: ");
                firstName = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(firstName))
                {
                    Console.WriteLine("Error! First name cannot be empty.");
                    Logger.Log("Error adding person: First name is empty.");
                }
                else
                {
                    firstNameIsValid = true;
                }
            } while (!firstNameIsValid);

            string lastName = string.Empty;
            bool lastNameIsValid = false;
            do
            {
                Console.Write("Enter the Last Name: ");
                lastName = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(lastName))
                {
                    Console.WriteLine("Error! Last name cannot be empty.");
                    Logger.Log("Error adding person: Last name is empty.");
                }
                else
                {
                    lastNameIsValid = true;
                }
            } while (!lastNameIsValid);

            string phone = string.Empty;
            bool phoneIsValid = false;
            do
            {
                Console.Write("Enter the Phone (digits only, e.g., 1234567890): ");
                phone = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, @"^\d+$"))
                {
                    Console.WriteLine("Error! Phone must contain only digits and cannot be empty.");
                    Logger.Log($"Error adding person: Phone '{phone}' is invalid.");
                }
                else
                {
                    phoneIsValid = true;
                }
            } while (!phoneIsValid);

            string city = string.Empty;
            Console.Write("Enter the City: ");
            city = Console.ReadLine() ?? string.Empty;

            decimal newBalance;
            bool balanceIsValid = false;
            do
            {
                Console.Write("Enter the Balance (must be a positive number or zero): ");
                var balanceInput = Console.ReadLine();
                if (decimal.TryParse(balanceInput, NumberStyles.Currency | NumberStyles.Number, CultureInfo.InvariantCulture, out newBalance))
                {
                    if (newBalance < 0)
                    {
                        Console.WriteLine("Error! Balance cannot be negative.");
                        Logger.Log($"Error adding person: Balance '{balanceInput}' is negative.");
                    }
                    else
                    {
                        balanceIsValid = true;
                    }
                }
                else
                {
                    Console.WriteLine("Error! Balance must be a valid number.");
                    Logger.Log($"Error adding person: Balance '{balanceInput}' is not a number.");
                }
            } while (!balanceIsValid);

            var newPerson = new Person
            {
                Id = newId,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                City = city,
                Balance = newBalance
            };

            readPeople.Add(newPerson);
            Logger.Log($"Person added successfully: ID={newPerson.Id}, Name='{newPerson.FirstName} {newPerson.LastName}'.");
            Console.WriteLine("Person added successfully.");
            break;

        case "3":
            Logger.Log("User selected 'Save People Changes'.");
            SaveChanges();
            break;

        case "4":
            Logger.Log("User selected 'Edit Person'.");
            Console.Write("Enter the ID of the person to edit: ");
            string? idToEditInput = Console.ReadLine();
            int idToEdit;

            if (!int.TryParse(idToEditInput, out idToEdit))
            {
                Console.WriteLine("Error! The ID must be a valid number.");
                Logger.Log($"Error editing person: ID '{idToEditInput}' is not a number.");
                break;
            }

            Person? personToEdit = readPeople.FirstOrDefault(p => p.Id == idToEdit);

            if (personToEdit == null)
            {
                Console.WriteLine($"Error! No person found with ID '{idToEdit}'.");
                Logger.Log($"Error editing person: No person found with ID '{idToEdit}'.");
                break;
            }

            Console.WriteLine("\n--- Editing Person (Press ENTER to keep current value) ---");
            Console.WriteLine($"Current ID: {personToEdit.Id}");
            Logger.Log($"Editing person with ID: {personToEdit.Id}");

            Console.Write($"Current First Name: {personToEdit.FirstName}. New First Name: ");
            string? newFirstNameInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(newFirstNameInput))
            {
                if (string.IsNullOrWhiteSpace(newFirstNameInput))
                {
                    Console.WriteLine("Error! First name cannot be just spaces. Keeping previous value.");
                    Logger.Log($"Error editing person (first name): '{newFirstNameInput}' is just spaces. Keeping '{personToEdit.FirstName}'.");
                }
                else
                {
                    personToEdit.FirstName = newFirstNameInput;
                    Logger.Log($"First Name of ID {personToEdit.Id} updated to '{personToEdit.FirstName}'.");
                }
            }

            Console.Write($"Current Last Name: {personToEdit.LastName}. New Last Name: ");
            string? newLastNameInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(newLastNameInput))
            {
                if (string.IsNullOrWhiteSpace(newLastNameInput))
                {
                    Console.WriteLine("Error! Last name cannot be just spaces. Keeping previous value.");
                    Logger.Log($"Error editing person (last name): '{newLastNameInput}' is just spaces. Keeping '{personToEdit.LastName}'.");
                }
                else
                {
                    personToEdit.LastName = newLastNameInput;
                    Logger.Log($"Last Name of ID {personToEdit.Id} updated to '{personToEdit.LastName}'.");
                }
            }

            string? newPhoneInput = string.Empty;
            bool phoneEditIsValid = false;
            do
            {
                Console.Write($"Current Phone: {personToEdit.Phone}. New Phone (digits only): ");
                newPhoneInput = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrEmpty(newPhoneInput))
                {
                    phoneEditIsValid = true;
                }
                else
                {
                    if (!Regex.IsMatch(newPhoneInput, @"^\d+$"))
                    {
                        Console.WriteLine("Error! Phone must contain only digits.");
                        Logger.Log($"Error editing person (phone): '{newPhoneInput}' is invalid. Keeping '{personToEdit.Phone}'.");
                    }
                    else
                    {
                        personToEdit.Phone = newPhoneInput;
                        phoneEditIsValid = true;
                        Logger.Log($"Phone of ID {personToEdit.Id} updated to '{personToEdit.Phone}'.");
                    }
                }
            } while (!phoneEditIsValid);

            Console.Write($"Current City: {personToEdit.City}. New City: ");
            string? newCityInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(newCityInput))
            {
                personToEdit.City = newCityInput;
                Logger.Log($"City of ID {personToEdit.Id} updated to '{personToEdit.City}'.");
            }

            string? newBalanceInput = string.Empty;
            decimal newBalanceEdit;
            bool balanceEditIsValid = false;
            do
            {
                Console.Write($"Current Balance: {personToEdit.Balance:C2}. New Balance (positive number or zero): ");
                newBalanceInput = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrEmpty(newBalanceInput))
                {
                    balanceEditIsValid = true;
                }
                else
                {
                    if (decimal.TryParse(newBalanceInput, NumberStyles.Currency | NumberStyles.Number, CultureInfo.InvariantCulture, out newBalanceEdit))
                    {
                        if (newBalanceEdit < 0)
                        {
                            Console.WriteLine("Error! Balance cannot be negative. Keeping previous value.");
                            Logger.Log($"Error editing person (balance): '{newBalanceInput}' is negative. Keeping '{personToEdit.Balance}'.");
                        }
                        else
                        {
                            personToEdit.Balance = newBalanceEdit;
                            balanceEditIsValid = true;
                            Logger.Log($"Balance of ID {personToEdit.Id} updated to '{personToEdit.Balance}'.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error! Balance must be a valid number. Keeping previous value.");
                        Logger.Log($"Error editing person (balance): '{newBalanceInput}' is not a number. Keeping '{personToEdit.Balance}'.");
                    }
                }
            } while (!balanceEditIsValid);

            Console.WriteLine($"Person with ID {personToEdit.Id} updated successfully.");
            Logger.Log($"Person with ID {personToEdit.Id} updated successfully.");
            break;

        case "5":
            Logger.Log("User selected 'Delete Person'.");
            Console.Write("Enter the ID of the person to delete: ");
            string? idToDeleteInput = Console.ReadLine();
            int idToDelete;

            if (!int.TryParse(idToDeleteInput, out idToDelete))
            {
                Console.WriteLine("Error! The ID must be a valid number.");
                Logger.Log($"Error deleting person: ID '{idToDeleteInput}' is not a number.");
                break;
            }

            Person? personToDelete = readPeople.FirstOrDefault(p => p.Id == idToDelete);

            if (personToDelete == null)
            {
                Console.WriteLine($"Error! No person found with ID '{idToDelete}'.");
                Logger.Log($"Error deleting person: No person found with ID '{idToDelete}'.");
                break;
            }

            Console.WriteLine("\n--- Person Found ---");
            Console.WriteLine($"ID: {personToDelete.Id}");
            Console.WriteLine($"First Name: {personToDelete.FirstName}");
            Console.WriteLine($"Last Name: {personToDelete.LastName}");
            Console.WriteLine($"Phone: {personToDelete.Phone}");
            Console.WriteLine($"City: {personToDelete.City}");
            Console.WriteLine($"Balance: {personToDelete.Balance:C2}");
            Console.WriteLine("--------------------------");

            Console.Write($"Are you sure you want to delete '{personToDelete.FirstName} {personToDelete.LastName}' (ID: {personToDelete.Id})? (Y/N): ");
            string? confirmation = Console.ReadLine()?.Trim().ToUpper();

            if (confirmation == "Y")
            {
                readPeople.Remove(personToDelete);
                Console.WriteLine($"Person with ID {personToDelete.Id} deleted successfully.");
                Logger.Log($"Person deleted successfully: ID={personToDelete.Id}, Name='{personToDelete.FirstName} {personToDelete.LastName}'.");
            }
            else
            {
                Console.WriteLine("Deletion operation cancelled.");
                Logger.Log($"Deletion of person with ID {personToDelete.Id} cancelled.");
            }
            break;

        case "6":
            Logger.Log("User selected 'Display City Balance Report'.");
            Console.WriteLine("\n--- CITY BALANCE REPORT ---");
            Console.WriteLine("=======================================");

            var peopleByCity = readPeople
                                .GroupBy(p => p.City)
                                .OrderBy(g => g.Key);

            decimal grandTotalBalance = 0m;

            if (!readPeople.Any())
            {
                Console.WriteLine("No people registered to generate the report.");
                Logger.Log("Report generated: No people registered.");
            }
            else
            {
                foreach (var cityGroup in peopleByCity)
                {
                    Console.WriteLine($"\nCity: {cityGroup.Key}");
                    Console.WriteLine("ID\tFirst Name\tLast Name\tBalance");
                    Console.WriteLine("--\t-----------\t-----------\t-----------");

                    decimal cityTotalBalance = 0m;
                    foreach (var person in cityGroup.OrderBy(p => p.LastName).ThenBy(p => p.FirstName))
                    {
                        Console.WriteLine($"{person.Id}\t{person.FirstName,-15}\t{person.LastName,-15}\t{person.Balance,10:C2}");
                        cityTotalBalance += person.Balance;
                    }
                    Console.WriteLine("\t\t\t\t\t=======");
                    Console.WriteLine($"Total: {cityGroup.Key}\t\t\t\t{cityTotalBalance,10:C2}");
                    grandTotalBalance += cityTotalBalance;
                }

                Console.WriteLine("\n\t\t\t\t\t=======");
                Console.WriteLine($"Grand Total:\t\t\t\t{grandTotalBalance,10:C2}");
                Console.WriteLine("=======================================");
                Logger.Log($"City balance report generated. Grand Total: {grandTotalBalance:C2}");
            }
            break;

        case "0":
            Logger.Log("User selected 'Exit' from main menu.");
            break;
    }
} while (opc != "0");
SaveChanges();

Logger.Log("Application terminated.");

void SaveChanges()
{
    helper.Write("people.csv", readPeople);
    Console.WriteLine("People data saved to people.csv.");
    Logger.Log("People data saved to 'people.csv'.");
}

string Menu()
{
    Console.WriteLine("=======================================");
    Console.WriteLine("1. Show content");
    Console.WriteLine("2. Add Person");
    Console.WriteLine("3. Save Changes");
    Console.WriteLine("4. Edit Person");
    Console.WriteLine("5. Delete Person");
    Console.WriteLine("6. Display City Balance Report");
    Console.WriteLine("0. Exit");
    Console.Write("Choose an option: ");
    return Console.ReadLine() ?? "0";
}

List<User> LoadUsers(string filePath)
{
    List<User> userList = new List<User>();
    if (File.Exists(filePath))
    {
        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length == 3)
            {
                userList.Add(new User
                {
                    Username = parts[0],
                    Password = parts[1],
                    IsActive = bool.Parse(parts[2])
                });
            }
        }
        Logger.Log($"Users loaded from '{filePath}'.");
    }
    else
    {
        Logger.Log($"Warning: User file '{filePath}' not found. Create one manually.");
        Console.WriteLine($"Warning: User file '{filePath}' not found. Create one manually.");
    }
    return userList;
}

void SaveUsers(string filePath, List<User> userList)
{
    var lines = userList.Select(u => u.ToString()).ToArray();
    File.WriteAllLines(filePath, lines);
    Console.WriteLine("User changes saved to Users.txt.");
    Logger.Log($"User changes saved to '{filePath}'.");
}

string ReadPassword()
{
    string password = "";
    ConsoleKeyInfo key;
    do
    {
        key = Console.ReadKey(true);

        if (char.IsLetterOrDigit(key.KeyChar) || char.IsPunctuation(key.KeyChar) || char.IsSymbol(key.KeyChar))
        {
            password += key.KeyChar;
            Console.Write("*");
        }
        else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password = password.Substring(0, password.Length - 1);
            Console.Write("\b \b");
        }
    } while (key.Key != ConsoleKey.Enter);

    Console.WriteLine();
    return password;
}
