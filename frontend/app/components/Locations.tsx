import React from 'react';
import { View, StyleSheet } from 'react-native';
import { useApp } from '../lib/hooks';
import Button from './Button';

export default function Locations()
{

    const {locations,history}=useApp();

    return (
        <View style={styles.root}>

            {locations.locations.map(loc=>(
                <Button title={loc.Name} key={loc.Id} onPress={()=>history.push('/location/'+loc.Id)}/>
            ))}

            <Button primary title="Add Location" onPress={()=>history.push('/add-location')} icon="plus"/>

        </View>
    )

}

const styles=StyleSheet.create({
    root:{
        flex:1
    }
});
