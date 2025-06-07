using CVSWithLibary;
using System.Text.RegularExpressions;
using System.Globalization; // Asegúrate de que esta directiva esté presente para CultureInfo

// --- Lógica para CsvHelperExample y People (sin cambios significativos aquí) ---
var helper = new CsvHelperExample();
var readPeople = helper.Read("people.csv").ToList();
// --- Fin Lógica People ---

// --- Lógica para Users (Nueva) ---
const string usersFilePath = "Users.txt";
List<User> users = LoadUsers(usersFilePath); // Cargar usuarios al inicio
// --- Fin Lógica Users ---

// Registrar el inicio de la aplicación
Logger.Log("Aplicación iniciada.");

// --- Autenticación ---
bool authenticated = false;
int loginAttempts = 0;
const int maxLoginAttempts = 3;
User? currentUser = null; // Para almacenar el usuario que intenta iniciar sesión

while (!authenticated && loginAttempts < maxLoginAttempts)
{
    Console.WriteLine("=======================================");
    Console.WriteLine("Por favor, inicie sesión:");
    Console.Write("Usuario: ");
    string? username = Console.ReadLine();
    Console.Write("Contraseña: ");
    string? password = ReadPassword(); // Función para leer contraseña sin mostrarla

    currentUser = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

    if (currentUser != null)
    {
        if (!currentUser.IsActive)
        {
            Logger.Log($"Intento de inicio de sesión fallido para '{username}': Usuario bloqueado.");
            Console.WriteLine("¡Error! Su usuario está bloqueado. Contacte al administrador.");
            loginAttempts = maxLoginAttempts; // Forzar la salida del bucle
            break;
        }

        if (currentUser.Password == password) // ¡ATENCIÓN! En prod real, usar hashing
        {
            authenticated = true;
            Logger.SetCurrentUser(currentUser.Username); // Establecer el usuario actual para el registro
            Logger.Log($"Inicio de sesión exitoso para '{currentUser.Username}'.");
            Console.WriteLine($"¡Bienvenido, {currentUser.Username}!");
            Console.WriteLine("=======================================");
        }
        else
        {
            Logger.Log($"Intento de inicio de sesión fallido para '{username}': Contraseña incorrecta.");
            Console.WriteLine("¡Error! Contraseña incorrecta.");
            loginAttempts++;
        }
    }
    else
    {
        Logger.Log($"Intento de inicio de sesión fallido para '{username ?? "NULL_USERNAME"}': Usuario no encontrado."); // Manejo de username nulo
        Console.WriteLine("¡Error! Usuario no encontrado.");
        loginAttempts++;
    }

    if (!authenticated && loginAttempts < maxLoginAttempts)
    {
        Console.WriteLine($"Intentos restantes: {maxLoginAttempts - loginAttempts}");
    }
}

if (!authenticated)
{
    Console.WriteLine("=======================================");
    Console.WriteLine("Demasiados intentos fallidos. Saliendo del programa.");
    if (currentUser != null && currentUser.IsActive) // Si el usuario existe y no estaba bloqueado, lo bloqueamos
    {
        currentUser.IsActive = false;
        SaveUsers(usersFilePath, users); // Guardar el estado actualizado (bloqueado)
        Logger.Log($"El usuario '{currentUser.Username}' ha sido bloqueado debido a múltiples intentos fallidos.");
        Console.WriteLine($"El usuario '{currentUser.Username}' ha sido bloqueado.");
    }
    else if (currentUser == null)
    {
        // Esto cubre el caso donde se intentaron múltiples logins fallidos con un usuario NO EXISTENTE
        Logger.Log("Programa terminado debido a múltiples intentos de inicio de sesión fallidos (usuario no encontrado).");
    }
    // Registrar la salida del programa si la autenticación falla
    Logger.Log("Aplicación terminada debido a fallos de autenticación.");
    Console.WriteLine("=======================================");
    return; // Salir del programa si la autenticación falla
}
// --- Fin Autenticación ---


// --- Resto del programa (menú principal si la autenticación es exitosa) ---
// Registrar el acceso al menú principal
Logger.Log("Acceso al menú principal.");
var opc = "0";
do
{
    opc = Menu();
    Console.WriteLine("=======================================");
    switch (opc)
    {
        case "1":
            Logger.Log("El usuario ha seleccionado 'Mostrar contenido de Personas'.");
            foreach (var person in readPeople)
            {
                Console.WriteLine(person);
            }
            break;

        case "2":

            Logger.Log("El usuario ha seleccionado 'Añadir Persona'. Iniciando validaciones.");

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
                        Console.WriteLine("¡Error! Este ID ya existe. Por favor, ingrese un ID único.");
                        Logger.Log($"Error al añadir persona: ID '{idInput}' ya existe.");
                    }
                    else if (newId < 0) // Puedes añadir esta validación si los IDs deben ser positivos
                    {
                        Console.WriteLine("¡Error! El ID debe ser un número positivo.");
                        Logger.Log($"Error al añadir persona: ID '{idInput}' no es positivo.");
                    }
                    else
                    {
                        idIsValid = true;
                    }
                }
                else
                {
                    Console.WriteLine("¡Error! El ID debe ser un número válido.");
                    Logger.Log($"Error al añadir persona: ID '{idInput}' no es un número.");
                }
            } while (!idIsValid);

            string firstName = string.Empty;
            bool firstNameIsValid = false;
            do
            {
                Console.Write("Enter the First name: ");
                firstName = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(firstName))
                {
                    Console.WriteLine("¡Error! El nombre no puede estar vacío.");
                    Logger.Log("Error al añadir persona: Nombre vacío.");
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
                Console.Write("Enter the Last name: ");
                lastName = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(lastName))
                {
                    Console.WriteLine("¡Error! El apellido no puede estar vacío.");
                    Logger.Log("Error al añadir persona: Apellido vacío.");
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
                Console.Write("Enter the phone (digits only, e.g., 1234567890): ");
                phone = Console.ReadLine() ?? string.Empty;
                // Validación de teléfono: solo dígitos y no vacío
                if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, @"^\d+$"))
                {
                    Console.WriteLine("¡Error! El teléfono debe contener solo dígitos y no puede estar vacío.");
                    Logger.Log($"Error al añadir persona: Teléfono '{phone}' inválido.");
                }
                else
                {
                    phoneIsValid = true;
                }
            } while (!phoneIsValid);

            string city = string.Empty; // La ciudad no tenía validación específica en tu lista, pero la dejamos para entrada.
            Console.Write("Enter the city: ");
            city = Console.ReadLine() ?? string.Empty;
            // No hay validación para la ciudad en los requisitos, así que se acepta cualquier entrada.

            decimal newBalance;
            bool balanceIsValid = false;
            do
            {
                Console.Write("Enter the balance (must be a positive number or zero): ");
                var balanceInput = Console.ReadLine();
                // Usamos CultureInfo.InvariantCulture para asegurar que el punto sea el separador decimal (ej. 123.45)
                if (decimal.TryParse(balanceInput, NumberStyles.Currency | NumberStyles.Number, CultureInfo.InvariantCulture, out newBalance))
                {
                    if (newBalance < 0)
                    {
                        Console.WriteLine("¡Error! El saldo no puede ser negativo.");
                        Logger.Log($"Error al añadir persona: Saldo '{balanceInput}' negativo.");
                    }
                    else
                    {
                        balanceIsValid = true;
                    }
                }
                else
                {
                    Console.WriteLine("¡Error! El saldo debe ser un número válido.");
                    Logger.Log($"Error al añadir persona: Saldo '{balanceInput}' no es un número.");
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
            Logger.Log($"Persona añadida exitosamente: ID={newPerson.Id}, Nombre='{newPerson.FirstName} {newPerson.LastName}'.");
            Console.WriteLine("Persona añadida exitosamente.");
            break;

        case "3":
            Logger.Log("El usuario ha seleccionado 'Guardar cambios de Personas'.");
            SaveChanges();
            break;

        case "4":
            Logger.Log("El usuario ha seleccionado 'Editar Persona'.");
            Console.Write("Ingrese el ID de la persona a editar: ");
            string? idToEditInput = Console.ReadLine();
            int idToEdit;

            if (!int.TryParse(idToEditInput, out idToEdit))
            {
                Console.WriteLine("¡Error! El ID debe ser un número válido.");
                Logger.Log($"Error al editar persona: ID '{idToEditInput}' no es un número.");
                break; // Sale del case 4
            }

            Person? personToEdit = readPeople.FirstOrDefault(p => p.Id == idToEdit);

            if (personToEdit == null)
            {
                Console.WriteLine($"¡Error! No se encontró una persona con el ID '{idToEdit}'.");
                Logger.Log($"Error al editar persona: No se encontró el ID '{idToEdit}'.");
                break; // Sale del case 4
            }

            Console.WriteLine("\n--- Editando Persona (Presione ENTER para mantener el valor actual) ---");
            Console.WriteLine($"ID actual: {personToEdit.Id}");
            Logger.Log($"Editando persona con ID: {personToEdit.Id}");

            // Editar Nombre
            Console.Write($"Nombre actual: {personToEdit.FirstName}. Nuevo nombre: ");
            string? newFirstNameInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(newFirstNameInput)) // Si no se presionó ENTER vacío
            {
                if (string.IsNullOrWhiteSpace(newFirstNameInput))
                {
                    Console.WriteLine("¡Error! El nombre no puede ser solo espacios. Se mantiene el valor anterior.");
                    Logger.Log($"Error al editar persona (nombre): '{newFirstNameInput}' es solo espacios. Se mantiene '{personToEdit.FirstName}'.");
                }
                else
                {
                    personToEdit.FirstName = newFirstNameInput;
                    Logger.Log($"Nombre de ID {personToEdit.Id} actualizado a '{personToEdit.FirstName}'.");
                }
            }

            // Editar Apellido
            Console.Write($"Apellido actual: {personToEdit.LastName}. Nuevo apellido: ");
            string? newLastNameInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(newLastNameInput))
            {
                if (string.IsNullOrWhiteSpace(newLastNameInput))
                {
                    Console.WriteLine("¡Error! El apellido no puede ser solo espacios. Se mantiene el valor anterior.");
                    Logger.Log($"Error al editar persona (apellido): '{newLastNameInput}' es solo espacios. Se mantiene '{personToEdit.LastName}'.");
                }
                else
                {
                    personToEdit.LastName = newLastNameInput;
                    Logger.Log($"Apellido de ID {personToEdit.Id} actualizado a '{personToEdit.LastName}'.");
                }
            }

            // Editar Teléfono
            string? newPhoneInput = string.Empty;
            bool phoneEditIsValid = false;
            do
            {
                Console.Write($"Teléfono actual: {personToEdit.Phone}. Nuevo teléfono (solo dígitos): ");
                newPhoneInput = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrEmpty(newPhoneInput)) // Si es ENTER vacío, mantiene el valor
                {
                    phoneEditIsValid = true; // El valor anterior es válido
                }
                else // Si se ingresó algo, validar
                {
                    if (!Regex.IsMatch(newPhoneInput, @"^\d+$"))
                    {
                        Console.WriteLine("¡Error! El teléfono debe contener solo dígitos.");
                        Logger.Log($"Error al editar persona (teléfono): '{newPhoneInput}' inválido. Se mantiene '{personToEdit.Phone}'.");
                    }
                    else
                    {
                        personToEdit.Phone = newPhoneInput;
                        phoneEditIsValid = true;
                        Logger.Log($"Teléfono de ID {personToEdit.Id} actualizado a '{personToEdit.Phone}'.");
                    }
                }
            } while (!phoneEditIsValid);

            // Editar Ciudad (sin validación específica, solo mantener si es vacío)
            Console.Write($"Ciudad actual: {personToEdit.City}. Nueva ciudad: ");
            string? newCityInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(newCityInput))
            {
                personToEdit.City = newCityInput;
                Logger.Log($"Ciudad de ID {personToEdit.Id} actualizada a '{personToEdit.City}'.");
            }

            // Editar Balance
            string? newBalanceInput = string.Empty;
            decimal newBalanceEdit;
            bool balanceEditIsValid = false;
            do
            {
                Console.Write($"Saldo actual: {personToEdit.Balance:C2}. Nuevo saldo (número positivo o cero): ");
                newBalanceInput = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrEmpty(newBalanceInput)) // Si es ENTER vacío, mantiene el valor
                {
                    balanceEditIsValid = true; // El valor anterior es válido
                }
                else // Si se ingresó algo, validar
                {
                    if (decimal.TryParse(newBalanceInput, NumberStyles.Currency | NumberStyles.Number, CultureInfo.InvariantCulture, out newBalanceEdit))
                    {
                        if (newBalanceEdit < 0)
                        {
                            Console.WriteLine("¡Error! El saldo no puede ser negativo. Se mantiene el valor anterior.");
                            Logger.Log($"Error al editar persona (saldo): '{newBalanceInput}' negativo. Se mantiene '{personToEdit.Balance}'.");
                        }
                        else
                        {
                            personToEdit.Balance = newBalanceEdit;
                            balanceEditIsValid = true;
                            Logger.Log($"Saldo de ID {personToEdit.Id} actualizado a '{personToEdit.Balance}'.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("¡Error! El saldo debe ser un número válido. Se mantiene el valor anterior.");
                        Logger.Log($"Error al editar persona (saldo): '{newBalanceInput}' no es un número. Se mantiene '{personToEdit.Balance}'.");
                    }
                }
            } while (!balanceEditIsValid);

            Console.WriteLine($"Persona con ID {personToEdit.Id} actualizada correctamente.");
            Logger.Log($"Persona con ID {personToEdit.Id} actualizada correctamente.");
            break;

        case "5":
            Logger.Log("El usuario ha seleccionado 'Eliminar Persona'.");
            Console.Write("Ingrese el ID de la persona a eliminar: ");
            string? idToDeleteInput = Console.ReadLine();
            int idToDelete;

            if (!int.TryParse(idToDeleteInput, out idToDelete))
            {
                Console.WriteLine("¡Error! El ID debe ser un número válido.");
                Logger.Log($"Error al eliminar persona: ID '{idToDeleteInput}' no es un número.");
                break; // Sale del case 5
            }

            Person? personToDelete = readPeople.FirstOrDefault(p => p.Id == idToDelete);

            if (personToDelete == null)
            {
                Console.WriteLine($"¡Error! No se encontró una persona con el ID '{idToDelete}'.");
                Logger.Log($"Error al eliminar persona: No se encontró el ID '{idToDelete}'.");
                break; // Sale del case 5
            }

            // Mostrar datos de la persona encontrada
            Console.WriteLine("\n--- Persona Encontrada ---");
            Console.WriteLine($"ID: {personToDelete.Id}");
            Console.WriteLine($"Nombre: {personToDelete.FirstName} {personToDelete.LastName}");
            Console.WriteLine($"Teléfono: {personToDelete.Phone}");
            Console.WriteLine($"Ciudad: {personToDelete.City}");
            Console.WriteLine($"Saldo: {personToDelete.Balance:C2}"); // Formato de moneda
            Console.WriteLine("--------------------------");

            // Pedir confirmación
            Console.Write($"¿Está seguro que desea eliminar a '{personToDelete.FirstName} {personToDelete.LastName}' (ID: {personToDelete.Id})? (S/N): ");
            string? confirmation = Console.ReadLine()?.Trim().ToUpper();

            if (confirmation == "S")
            {
                readPeople.Remove(personToDelete);
                Console.WriteLine($"Persona con ID {personToDelete.Id} eliminada correctamente.");
                Logger.Log($"Persona eliminada correctamente: ID={personToDelete.Id}, Nombre='{personToDelete.FirstName} {personToDelete.LastName}'.");
            }
            else
            {
                Console.WriteLine("Operación de eliminación cancelada.");
                Logger.Log($"Eliminación de persona con ID {personToDelete.Id} cancelada.");
            }
            break;

        case "6":
            Logger.Log("El usuario ha seleccionado 'Mostrar informe con subtotales por Ciudad'.");
            Console.WriteLine("\n--- INFORME DE SALDOS POR CIUDAD ---");
            Console.WriteLine("=======================================");

            // Agrupar personas por ciudad
            var peopleByCity = readPeople
                                .GroupBy(p => p.City)
                                .OrderBy(g => g.Key); // Ordenar por nombre de ciudad

            decimal grandTotalBalance = 0m;

            if (!readPeople.Any())
            {
                Console.WriteLine("No hay personas registradas para generar el informe.");
                Logger.Log("Informe generado: No hay personas registradas.");
            }
            else
            {
                foreach (var cityGroup in peopleByCity)
                {
                    Console.WriteLine($"\nCiudad: {cityGroup.Key}");
                    Console.WriteLine("ID\tNombres\t\tApellidos\tSaldo");
                    Console.WriteLine("—\t———————\t\t———————\t\t———————");

                    decimal cityTotalBalance = 0m;
                    foreach (var person in cityGroup.OrderBy(p => p.LastName).ThenBy(p => p.FirstName)) // Ordenar personas dentro de cada ciudad
                    {
                        Console.WriteLine($"{person.Id}\t{person.FirstName,-15}\t{person.LastName,-15}\t{person.Balance,10:C2}");
                        cityTotalBalance += person.Balance;
                    }
                    Console.WriteLine("\t\t\t\t\t=======");
                    Console.WriteLine($"Total: {cityGroup.Key}\t\t\t\t{cityTotalBalance,10:C2}");
                    grandTotalBalance += cityTotalBalance;
                }

                Console.WriteLine("\n\t\t\t\t\t=======");
                Console.WriteLine($"Total General:\t\t\t\t{grandTotalBalance,10:C2}");
                Console.WriteLine("=======================================");
                Logger.Log($"Informe de saldos por ciudad generado. Total General: {grandTotalBalance:C2}");
            }
            break;

        case "0":
            Logger.Log("El usuario ha seleccionado 'Salir' del menú principal.");
            break; // No hay un break después de llamar a SaveChanges() al final del do-while
    }
} while (opc != "0");
SaveChanges(); // Guardar cambios al salir del menú principal (esto también se registra)

// Registrar la salida final de la aplicación
Logger.Log("Aplicación terminada.");

// --- Funciones Locales (sin cambios significativos en su lógica, solo añade logs) ---

void SaveChanges()
{
    helper.Write("people.csv", readPeople);
    Console.WriteLine("Cambios de personas guardados en people.csv.");
    Logger.Log("Cambios de personas guardados en 'people.csv'.");
}

string Menu()
{
    Console.WriteLine("=======================================");
    Console.WriteLine("1. Mostrar contenido de Personas");
    Console.WriteLine("2. Añadir Persona");
    Console.WriteLine("3. Guardar cambios de Personas");
    Console.WriteLine("4. Editar Persona");
    Console.WriteLine("5. Eliminar Persona");
    Console.WriteLine("6. Mostrar Informe por Ciudad"); // <-- NUEVA OPCIÓN
    Console.WriteLine("0. Salir");
    Console.Write("Elija una opción: ");
    return Console.ReadLine() ?? "0";
}

// Función para leer usuarios del archivo
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
        Logger.Log($"Usuarios cargados desde '{filePath}'.");
    }
    else
    {
        Logger.Log($"Advertencia: El archivo de usuarios '{filePath}' no se encontró. Cree uno manualmente.");
        Console.WriteLine($"Advertencia: El archivo de usuarios '{filePath}' no se encontró. Cree uno manualmente.");
    }
    return userList;
}

// Función para guardar usuarios en el archivo
void SaveUsers(string filePath, List<User> userList)
{
    var lines = userList.Select(u => u.ToString()).ToArray();
    File.WriteAllLines(filePath, lines);
    Console.WriteLine("Cambios de usuarios guardados en Users.txt.");
    Logger.Log($"Cambios de usuarios guardados en '{filePath}'.");
}

// Función para leer la contraseña sin mostrarla en consola
string ReadPassword()
{
    string password = "";
    ConsoleKeyInfo key;
    do
    {
        key = Console.ReadKey(true); // Lee la tecla sin mostrarla

        // Ignora teclas de control como Shift, Alt, Ctrl
        if (char.IsLetterOrDigit(key.KeyChar) || char.IsPunctuation(key.KeyChar) || char.IsSymbol(key.KeyChar))
        {
            password += key.KeyChar;
            Console.Write("*"); // Muestra un asterisco por cada carácter
        }
        else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password = password.Substring(0, password.Length - 1);
            Console.Write("\b \b"); // Borra el asterisco anterior
        }
    } while (key.Key != ConsoleKey.Enter); // Termina al presionar Enter

    Console.WriteLine(); // Nueva línea después de la contraseñaww
    return password;
}
