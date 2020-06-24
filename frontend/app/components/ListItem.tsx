import React from 'react';
import { Text, StyleSheet, TouchableOpacity } from 'react-native';

interface ListItemProps
{
    title?:string;
    onPress?:()=>void;
}

export default function ListItem({
    title,
    onPress
}:ListItemProps){

    return (
        <TouchableOpacity style={styles.item} onPress={onPress}>
            <Text style={styles.itemText}>{title}</Text>
        </TouchableOpacity>
    )

}

const styles=StyleSheet.create({
    item:{
        margin:10,
        padding:20,
        backgroundColor:'#ffffff44',
        borderRadius:10
    },
    itemText:{
        fontSize:18
    }
});
