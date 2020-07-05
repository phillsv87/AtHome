import React from 'react';
import { StyleSheet, TouchableOpacity, GestureResponderEvent, Text, View } from 'react-native';
import { primaryColor, primaryOverlayColor } from '../lib/style';
import { toBool } from '../CommonJs/commonUtils';
import RnIcon from '../CommonJs/RnIcon-rn';

interface ButtonProps
{
    title?:string;
    onPress?:(event: GestureResponderEvent)=>void;
    children?:any;
    primary?:boolean;
    icon?:string;
}

export default function Button({
    title,
    onPress,
    children,
    primary,
    icon
}:ButtonProps){

    if(!children){
        children=(
            <Text style={[styles.itemText,primary?styles.itemTextPrimary:null]}>{title}</Text>
        )
    }

    return (
        <TouchableOpacity style={[styles.btn,primary?styles.btnPrimary:null]} onPress={onPress}>
            
            {children}
            {toBool(icon)&&<View pointerEvents="none" style={styles.iconContainer}>
                <RnIcon size={22} icon={icon as string} color={primary?primaryColor:primaryOverlayColor}/>
            </View>}
        </TouchableOpacity>
    )

}

const styles=StyleSheet.create({
    btn:{
        borderRadius:100,
        borderWidth:2,
        borderStyle:'solid',
        borderColor:primaryColor,
        backgroundColor:primaryColor,
        marginVertical:10,
        flexDirection:'row',
        justifyContent:'center'
    },
    itemText:{
        fontSize:18,
        color:primaryOverlayColor,
        margin:20
    },

    btnPrimary:{
        backgroundColor:primaryOverlayColor
    },
    itemTextPrimary:{
        color:primaryColor
    },
    iconContainer:{
        position:'absolute',
        right:20,
        top:0,
        height:'100%',
        justifyContent:'center'
    }

});
