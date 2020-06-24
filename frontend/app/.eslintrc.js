module.exports = {
  root: true,
  parser: "@typescript-eslint/parser",
  parserOptions: {
    ecmaVersion: 2020, // Allows for the parsing of modern ECMAScript features
    sourceType: "module", // Allows for the use of imports
    ecmaFeatures: {
      jsx: true // Allows for the parsing of JSX
    }
  },
  settings: {
    react: {
      version: "detect" // Tells eslint-plugin-react to automatically detect the version of React to use
    }
  },
  extends: [
    "plugin:react/recommended",
    "plugin:react-hooks/recommended",
    "plugin:@typescript-eslint/recommended" // Uses the recommended rules from the @typescript-eslint/eslint-plugin
  ],
  plugins: [
    "react",
    "react-native",
    "react-hooks"
  ],
  env: {
    "react-native/react-native": true
  },
  rules: {
    "react-native/no-unused-styles": 1,
    "react-native/split-platform-components": 2,
    "react-native/no-inline-styles": 0,
    "react-native/no-color-literals": 0,
    "react-native/no-raw-text": 0,
    "react-native/no-single-element-style-arrays": 2,
    "@typescript-eslint/explicit-module-boundary-types":0,
    "@typescript-eslint/no-explicit-any":0,
    "@typescript-eslint/ban-ts-comment":0,
    "@typescript-eslint/no-inferrable-types":0,
    "react/no-unescaped-entities":0,
    "react-hooks/rules-of-hooks": 2,
    "react-hooks/exhaustive-deps": 2,
    "@typescript-eslint/no-non-null-assertion":0,
    "react/display-name":0,
    "@typescript-eslint/no-use-before-define":0,
    "@typescript-eslint/explicit-function-return-type":0,
    "@typescript-eslint/type-annotation-spacing":0,
    "@typescript-eslint/ban-ts-ignore":0,
    "@typescript-eslint/no-empty-function":0
  }
};
