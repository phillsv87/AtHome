import React from 'react';
import { View, StyleSheet, ActivityIndicator } from 'react-native';
import { absFill } from '../lib/style';

interface BusyProps
{
    touchable?:boolean;
    backgroundColor?:string;
}

export default function Busy({
    touchable=false,
    backgroundColor='#00000000'
}:BusyProps){

    return (
        <View pointerEvents={touchable?"auto":"none"} style={[styles.root,{backgroundColor}]}>
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
