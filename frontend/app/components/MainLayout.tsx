import React from 'react';
import { View, StyleSheet, StatusBar } from 'react-native';
import { Router } from './Router';
import LogUI from '../CommonJs/LogUI';

export default function MainLayout()
{

    return (
        <View style={styles.root}>
            <StatusBar barStyle="dark-content"/>
            <Router/>
            <LogUI
                infoColor="#2b8de0"
                warnColor="#f4921e"
                errorColor="#ec2424"
                textColor="#fff"/>
        </View>
    )

}

const styles=StyleSheet.create({
    root:{
        flex:1,
        backgroundColor:'#fff'
    }
});
