# BlackChat

Free, open-source communication platform with end-to-end encryption.

## About

BlackChat is a privacy-focused messaging application that puts security first. All messages are encrypted locally before being sent, ensuring that no third party can access your conversations. The application uses AES-256-CBC encryption with hardware-based key generation.

## Features

- End-to-end encryption (AES-256-CBC)
- Public chat rooms
- Private one-to-one messaging
- Group chats with invite codes
- Friend management system
- Automatic message refresh
- Hardware-based encryption keys
- Dark theme interface

## Security

All messages are encrypted using AES-256-CBC before leaving your device. Encryption keys are generated locally using:

- Hardware identifiers (CPU, Disk, MAC address)
- PBKDF2 with 100,000 iterations
- SHA-512 hashing
- HMAC for integrity verification

Each computer generates a unique encryption key. This means messages encrypted on one machine can only be decrypted on that same machine.

## Technology Stack

- C# with .NET Windows Forms
- Firebase Realtime Database
- AES-256-CBC encryption
- PBKDF2 key derivation

## Installation

### Prerequisites

- .NET 10.0 SDK or later
- Windows operating system

### Building from Source

1. Clone the repository:
```
git clone https://github.com/w1spo/BlackChat.git
```

2. Open the solution in Visual Studio 2022 or later

3. Restore NuGet packages:
```
dotnet restore
```

4. Build the project:
```
dotnet build
```

### Running the Application

1. Configure Firebase:
   - Create a Firebase project
   - Add your Firebase configuration to the application
   - Set up Firebase Realtime Database

2. Run the application:
```
dotnet run
```

## Usage

### Creating an Account

Launch the application and create a new account using the registration form. Choose a unique username and a strong password.

### Adding Friends

Use the "Add Friend" button in the main interface to add other users by their username.

### Creating Groups

Create groups using the "Create Group" button. Each group is assigned a unique code that you can share with others.

### Joining Groups

Use the "Join Group" button and enter the group code provided by the group creator.

### Sending Messages

Type your message in the input field at the bottom of the chat window and press Enter or click the Send button.

## License

This project is licensed under the GNU Affero General Public License v3.0 (AGPL-3.0).

You are free to:
- Use the software for any purpose
- Modify the software
- Distribute copies of the software

Under the following conditions:
- You must include the original copyright notice
- You must disclose the source code of any modified versions
- You must license modifications under the same terms

This license ensures that BlackChat remains free and open-source forever.

## Disclaimer

BlackChat is provided "as is" without warranty of any kind. The authors assume no responsibility for any damages or security breaches that may occur from using this software.

## Contributing

Contributions are welcome. Please ensure that:
- Code follows existing style conventions
- All encryption logic is thoroughly tested
- Security features are properly implemented

## Contact

For questions or feedback, please open an issue on GitHub.
