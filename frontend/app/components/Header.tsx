import React from 'react';
import { View, StyleSheet, Text } from 'react-native';
import RnIcon from '../CommonJs/RnIcon-rn';
import { primaryColor, primaryOverlayColor } from '../lib/style';
import BackButton from './BackButton';

interface HeaderProps
{
    title:string;
    icon:string;
    pop?:boolean;
}

export default function Header({
    title,
    icon,
    pop
}:HeaderProps){

    return (
        <View style={styles.root}>
            <RnIcon style={styles.icon} size={130} icon={icon as string}/>
            <Text style={styles.text}>{title}</Text>
            {pop&&<BackButton/>}
        </View>
    )

}

const styles=StyleSheet.create({
    root:{
        alignItems:'center',
        backgroundColor:primaryColor,
        borderRadius:20,
        marginBottom:20
    },
    icon:{
        color:primaryOverlayColor,
        marginTop:20,
    },
    text:{
        color:primaryOverlayColor,
        marginTop:10,
        marginBottom:20,
        letterSpacing:5,
        fontSize:18,
    },
    
});
