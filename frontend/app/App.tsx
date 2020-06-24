import React from 'react';
import { View, StyleSheet } from 'react-native';
import { useCreateApp, HsAppContext } from './lib/hooks';
import appConfig from './app-config';
import { HistoryContext } from './CommonJs/History-rn';
import MainLayout from './components/MainLayout';

export default function App()
{

    const [app]=useCreateApp(appConfig);

    if(!app){
        return null;
    }

    return (
        <HsAppContext.Provider value={app}>
        <HistoryContext.Provider value={app.history}>
            <MainLayout/>
        </HistoryContext.Provider>
        </HsAppContext.Provider>
    )

}