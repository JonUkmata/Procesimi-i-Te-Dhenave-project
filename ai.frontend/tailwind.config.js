/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{js,jsx,ts,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: '#5a67a0',
        secondary: '#9674c5',
        accent: '#db7093',
      },
      backgroundImage: {
        'gradient-primary': 'linear-gradient(135deg, #5a67a0 0%, #9674c5 50%, #db7093 100%)',
        'gradient-accent': 'linear-gradient(90deg, #db7093 0%, #9674c5 100%)',
      }
    },
  },
  plugins: [],
}