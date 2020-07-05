import React from 'react';
import { View, StyleSheet, TextInput as RNTextInput, Text } from 'react-native';
import { primaryColor, primaryOverlayColor, foregroundColor } from '../lib/style';
import { toBool } from '../CommonJs/commonUtils';

interface TextInputProps
{
    value:string|null|undefined;
    onChangeText:(value:string)=>void;
    placeholder?:string;
}

export default function TextInput({
    value,
    onChangeText,
    placeholder
}:TextInputProps){

    return (
        <View style={styles.root}>
            <RNTextInput
                placeholder={placeholder}
                style={styles.input}
                placeholderTextColor={foregroundColor+'33'}
                value={value||''}
                onChangeText={onChangeText}/>

            {toBool(value)&&<Text style={styles.placeholder}>{placeholder}</Text>}
        </View>
    )

}

const styles=StyleSheet.create({
    root:{
        borderRadius:100,
        borderWidth:2,
        borderStyle:'solid',
        borderColor:primaryColor,
        backgroundColor:primaryOverlayColor,
        marginVertical:10,
    },
    input:{
        color:foregroundColor,
        padding:20
    },
    placeholder:{
        position:'absolute',
        left:23,
        top:7,
        color:foregroundColor+'44',
        fontSize:8
    }
});
