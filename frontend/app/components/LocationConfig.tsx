import React, { useState, useCallback } from 'react';
import { View, StyleSheet, Text, TextInput, Button } from 'react-native';
import { useApp } from '../lib/hooks';
import { inputStyle } from '../lib/style';
import { useMounted } from '../CommonJs/Hooks';

interface LocationConfigProps
{
    locationId?:string;
}

export default function LocationConfig({
    locationId
}:LocationConfigProps){

    const {locations,history}=useApp();

    const location=locations.getLocation(locationId);

    const [confirmDelete,setConfirmDelete]=useState(false);
    const mt=useMounted();
    const deleteLocation=useCallback(async ()=>{

        if(!location){
            return;
        }

        if(await locations.deleteLocationAsync(location.Id) && mt.mounted){
            history.reset('/');
        }

    },[mt,locations,location,history]);

    if(locationId===undefined){
        return null;
    }

    if(!location){
        return <Text>Invalid Location Id - {locationId}</Text>;
    }

    return (
        <View style={styles.root}>

            <TextInput style={inputStyle} placeholderTextColor="#bbb" placeholder="Name" value={location.Name}/>
            <TextInput style={inputStyle} placeholderTextColor="#bbb" placeholder="Url" value={location.ApiBaseUrl} />

            {!confirmDelete&&<Button title="Delete" onPress={()=>setConfirmDelete(true)} />}
            {confirmDelete&&<Button title="Really Delete" onPress={deleteLocation} />}
        </View>
    )

}

const styles=StyleSheet.create({
    root:{
        flex:1
    }
});
