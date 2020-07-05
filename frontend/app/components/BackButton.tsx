import React from 'react';
import { StyleSheet, TouchableOpacity, StyleProp, ViewStyle } from 'react-native';
import RnIcon from '../CommonJs/RnIcon-rn';
import { useApp } from '../lib/hooks';
import { primaryColor, primaryOverlayColor } from '../lib/style';

interface BackButtonProps
{
    style?:StyleProp<ViewStyle>;
    iconColor?:string;
}

export default function BackButton({
    style,
    iconColor
}:BackButtonProps){

    const {history}=useApp();

    return (
        <TouchableOpacity style={[styles.pop,style]} onPress={()=>history.pop()}>
            <RnIcon icon="ft:chevron-left" size={30} color={iconColor||primaryColor} style={styles.popIcon}/>
        </TouchableOpacity>
    )

}

const styles=StyleSheet.create({
    pop:{
        position:'absolute',
        left:20,
        top:20,
        backgroundColor:primaryOverlayColor,
        borderRadius:100,
        width:40,
        height:40,
        alignItems:'center',
        justifyContent:'center'
    },
    popIcon:{
        transform:[{translateX:-1},{translateY:1}]
    }
});
