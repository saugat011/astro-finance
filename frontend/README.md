# Astro Finance - Frontend

A modern financial management system for microfinance institutions built with Next.js, TypeScript, and Tailwind CSS.

## Project Structure

This project follows clean architecture principles to ensure separation of concerns and maintainability.

```
astro-finance/
├── app/                  # Next.js App Router pages
│   ├── (auth)/           # Authentication routes (login, etc.)
│   ├── (dashboard)/      # Dashboard and protected routes
│   └── layout.tsx        # Root layout with providers
├── components/           # UI components
│   ├── auth/             # Authentication components
│   ├── calculator/       # Loan calculator components
│   ├── dashboard/        # Dashboard components
│   ├── interest/         # Interest rate components
│   ├── journal/          # Journal entry components
│   ├── layout/           # Layout components (sidebar, topbar)
│   ├── loans/            # Loan management components
│   ├── print/            # Print preview components
│   ├── reports/          # Report components
│   ├── sms/              # SMS components
│   ├── ui/               # Reusable UI components
│   └── users/            # User management components
├── hooks/                # Custom React hooks
│   ├── use-api.ts        # API data fetching hook
│   └── use-toast.ts      # Toast notification hook
├── lib/                  # Library code
│   ├── api/              # API integration
│   │   ├── services/     # API service modules
│   │   ├── types/        # API type definitions
│   │   └── axios.ts      # Axios instance with interceptors
│   ├── context/          # React context providers
│   │   └── auth-context.tsx # Authentication context
│   └── utils/            # Utility functions
│       ├── api-utils.ts  # API utilities
│       └── format.ts     # Data formatting utilities
├── public/               # Static assets
├── styles/               # Global styles
│   └── globals.css       # Global CSS
├── middleware.ts         # Next.js middleware for auth protection
├── next.config.mjs       # Next.js configuration
├── package.json          # Project dependencies
├── tailwind.config.ts    # Tailwind CSS configuration
└── tsconfig.json         # TypeScript configuration
```

## Architecture

The project follows a clean architecture approach with the following layers:

1. **Presentation Layer** (components, pages)
   - UI components and pages that handle user interaction
   - Uses hooks and contexts to access application state and services

2. **Application Layer** (hooks, contexts)
   - Custom hooks for business logic
   - Context providers for state management
   - Handles communication between UI and domain layer

3. **Domain Layer** (lib/api/services, lib/api/types)
   - Core business logic
   - Service modules for API communication
   - Type definitions for domain entities

4. **Infrastructure Layer** (lib/api/axios, middleware)
   - External services integration
   - HTTP client configuration
   - Authentication middleware

## API Integration

The frontend is designed to work with a .NET backend API. The integration is handled through:

- Axios instance with interceptors for authentication and error handling
- Strongly typed API services for each domain entity
- Comprehensive type definitions for API requests and responses

## Getting Started

1. Clone the repository
2. Install dependencies:
   ```bash
   npm install
   ```
3. Set up environment variables:
   ```
   NEXT_PUBLIC_API_URL=http://your-dotnet-api-url
   ```
4. Run the development server:
   ```bash
   npm run dev
   ```

## Authentication

The application uses token-based authentication:

- JWT tokens stored in localStorage
- Auth context provider for managing user state
- Protected routes via Next.js middleware
- Automatic redirection to login for unauthenticated users

## State Management

- React Context API for global state (auth, theme)
- React Query for server state management
- Form state managed with React Hook Form
- Zod for form validation

## Styling

- Tailwind CSS for utility-first styling
- Shadcn UI components for consistent design
- Dark mode support via next-themes

## Best Practices

- TypeScript for type safety
- ESLint and Prettier for code quality
- Responsive design for all screen sizes
- Accessibility compliance
- Performance optimization