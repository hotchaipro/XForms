import { dotnet } from './_framework/dotnet.js'

// Starts .NET and runs the app's Main, which shows it in the page
const { runMain } = await dotnet.create();
await runMain();
