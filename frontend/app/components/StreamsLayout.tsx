import React from 'react';
import { View, StyleSheet } from 'react-native';
import BackButton from './BackButton';
import Stream from './Stream';

interface StreamsLayoutProps
{
    locationId?:string;
    streamIds?:number[];
}

export default function StreamsLayout({
    locationId,
    streamIds
}:StreamsLayoutProps){

    return (
        <View style={styles.root}>

            {streamIds?.map((id,index)=><Stream key={id+':'+index} locationId={locationId} streamId={id} />)}

            <BackButton iconColor="#000" style={styles.back}/>
        </View>
    )

}

const styles=StyleSheet.create({
    root:{
        flex:1,
        backgroundColor:'#000'
    },
    back:{
        top:40
    }
});
