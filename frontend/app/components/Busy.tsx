import React from 'react';
import { View, StyleSheet, ActivityIndicator } from 'react-native';
import { absFill } from '../lib/style';

interface BusyProps
{
    
}

export default function Busy({

}:BusyProps){

    return (
        <View pointerEvents="none" style={styles.root}>
            <ActivityIndicator/>
        </View>
    )

}

const styles=StyleSheet.create({
    root:{
        ...absFill,
        justifyContent:'center',
        alignItems:'center'
    }
});
