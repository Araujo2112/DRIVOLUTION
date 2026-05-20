/** @type {import('tailwindcss').Config} */
export default {
    darkMode: 'class',
    content: [
        "./index.html",
        "./src/**/*.{vue,js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            colors: {
                // Superfícies — claro (50=mais claro, 950=mais escuro)
                background: {
                    50:  '#ffffff',
                    100: '#f8fafc',
                    200: '#f1f5f9',
                    300: '#e2e8f0',
                    400: '#cbd5e1',
                    500: '#94a3b8',
                    600: '#64748b',
                    700: '#334155',
                    800: '#1e293b',
                    900: '#0f172a',
                    950: '#020617',
                },
                // Cor oficial Drivolution — #3E55F2
                primary: {
                    50:  '#eef0fe',
                    100: '#d6dafd',
                    200: '#adb5fb',
                    300: '#8490f8',
                    400: '#6270f5',
                    500: '#3E55F2',
                    600: '#3248d9',
                    700: '#2639b5',
                    800: '#1d2b91',
                    900: '#141f6e',
                    950: '#0b1247',
                },
                // Semânticas — para estados
                success: {
                    100: '#dcfce7',
                    500: '#22c55e',
                    700: '#15803d',
                },
                warning: {
                    100: '#fef9c3',
                    500: '#eab308',
                    700: '#a16207',
                },
                danger: {
                    100: '#fee2e2',
                    500: '#ef4444',
                    700: '#b91c1c',
                },
            },
            fontFamily: {
                sans: ['Roboto', 'sans-serif'],
            },
        },
    },
    plugins: [],
}