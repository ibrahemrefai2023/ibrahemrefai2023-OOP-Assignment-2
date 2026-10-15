namespace LibrarySystem;

public class Person
{
    public string PersonId { get; }
    public string FullName { get; }
    public string Phone { get; }

    protected Person(string personId, string fullName, string phone)
    {
        if (string.IsNullOrWhiteSpace(personId)) throw new ArgumentException("Person ID is required.");
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required.");
        if (string.IsNullOrWhiteSpace(phone)) throw new ArgumentException("Phone is required.");

        PersonId = personId;
        FullName = fullName;
        Phone = phone;
    }
}
