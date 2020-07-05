import React from 'react';
import { View, StyleSheet, Text, TouchableOpacity } from 'react-native';
import RnIcon from '../CommonJs/RnIcon-rn';
import { primaryColor, primaryOverlayColor } from '../lib/style';
import { useApp } from '../lib/hooks';

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

    const {history}=useApp();

    return (
        <View style={styles.root}>
            <RnIcon style={styles.icon} size={130} icon={icon as string}/>
            <Text style={styles.text}>{title}</Text>
            {pop&&<TouchableOpacity style={styles.pop} onPress={()=>history.pop()}>
                <RnIcon icon="ft:chevron-left" size={30} color={primaryColor} style={styles.popIcon}/>
            </TouchableOpacity>}
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
